// Copyright 2013 Cultural Heritage Agency of the Netherlands, Dutch National Military Museum and Trezorix bv
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

Ext.define('DC.controller.DocumentController', {
	extend: 'Ext.app.Controller',

	models: ['DocumentModel'],
	stores: ['DocumentStore'],

	views: [
		'document.Index',
		'document.Grid'
	],

	refs: [
		{ ref: 'upload', selector: '#upload' },
		{ ref: 'review', selector: '#review' },
		{ ref: 'grid', selector: 'documentgrid' }
	],

	init: function () {

		this.classNames = {
			MATCH: 'textmatch',
			CLICK: 'textmatch-highlight',
			HOVER: 'textmatch-highlight',
			SELECT: 'textmatch-selected'
		};

		this.control({
			'filefield': {
				'change': this.uploadDocument,
				'afterrender': this.onRenderFileField
			},
			'button[action="check-url"]': {
				'click': this.checkUrl
			},
			'button[action="skip"]': {
				'click': this.deleteDocument
			},
			'button[action="export"]': {
				'click': this.exportDocuments
			},
			'button[action="clean"]': {
				'click': this.deleteDocumentSet
			}
		});
	},

	// Private
	onRenderFileField: function (field) {
		field.button.on('click', this.onClickUploadButton, this);
	},

	// Private
	onClickUploadButton: function (uploadButton, e) {
		var saveRequired = this.getSaveRequired();

		if (saveRequired) {
			e.stopEvent();

			Ext.Msg.confirm('Unsaved metadata', 'Do you want to save the metadata selection?', function (button) {
				if (button === "yes") {
					this.getController('SuggestionController').saveSuggestions();
					this.getController('SuggestionController').setSaveRequired(false);
				}
				else {
					this.getController('SuggestionController').setSaveRequired(false);
				}

			}, this);
		}
	},

	// Check a given url
	checkUrl: function (button) {
		var url = button.prev('textfield').getValue(),
				jobLabel = this.getController('JobController').getJobLabel();
		
		if (url) {
			Ext.Ajax.request({
				scope: this,
				url: 'Document/DownloadWebpage',
				params: {
					url: url,
					jobLabel: jobLabel
				},
				callback: function (options, success, response) {
					if (success) {
						var json = Ext.decode(response.responseText);
						this.saveUpload(json.id);
					}
					else {
						Ext.Msg.alert('Problem checking url', response.statusText);
					}
				}
			});
		}
	},

	// Upload document to the server
	uploadDocument: function (field, value) {
		var form = field.up('form').getForm(),
				jobLabel = this.getController('JobController').getJobLabel();

		if (form.isValid()) {
			form.submit({
				scope: this,
				params: {
					jobLabel: jobLabel
				},
				url: 'Document/Upload',
				waitMsg: 'Uploading document...',
				success: function (panel, response) {
					var id = response.result.id;

					this.saveUpload(id);
				}
			});
		}
	},

	// Save document id and start checking status.
	saveUpload: function (id) {
		this.setDocumentId(id);
		this.getStatus(id);
	},

	// Get document status from the server
	getStatus: function (id) {
		id = id || this.getDocumentId();

		if (!Ext.Msg.isVisible()) {
			Ext.Msg.wait('Checking document, please wait', 'Checking document');
		}

		Ext.Ajax.request({
			scope: this,
			url: 'Document/Status',
			params: {
				id: id
			},
			callback: this.checkStatus
		});
	},

	// Check the document status response
	// In case of error give feedback and delete document.
	checkStatus: function (options, success, response) {
		var me = this,
				id = options.params.id,
				json = Ext.decode(response.responseText),
				isReady = json.statusText === 'ReadyForReview',
				isOk = json.status > 0 && !json.conversionError;

		if (!success || !isOk) {
			Ext.Msg.alert('Error', 'The document cannot be checked.');
			me.deleteDocument();
		}
		else if (isReady) {
			Ext.Msg.close();
			me.getResults(id);
		}
		else {
			// Check the status again after 1 second
			setTimeout(function () {
				me.getStatus(id);
			}, 1000);
		}
	},

	// Get metadata suggestions and review document
	getResults: function (id) {
		this.getController('SuggestionController').getSuggestions(id);
		this.getReviewDocument(id);
	},

	// Load the review document into the panel's body
	getReviewDocument: function (id) {
		this.getReview().body.load({
			url: 'Review/AnalysisResultRendering',
			params: {
				id: id
			},
			scope: this,
			callback: this.setTextMatches
		});
	},

	// Get text matches from document
	setTextMatches: function () {
		var cls = this.classNames['MATCH'],
				root = this.getReview().body.dom;

		this.textMatches = Ext.select('.' + cls, root);
	},

	// Add candidate text-match to the document
	addCandidateTextMatch: function (literal, node) {
		var re,
				value,
				newValue,
				me = this,
				root = this.getReview().body.dom,
				nodes = Ext.DomQuery.jsSelect('p', root);

		Ext.Array.forEach(nodes, function (node, i) {
			re = new RegExp('\\b(' + literal + ')\\b', 'gi');
			value = node.innerHTML;
			newValue = value.replace(re, '<span data-literal="' + literal + '" class="' + me.classNames['MATCH'] + '">$1</span>');

			if (value !== newValue) {
				node.innerHTML = newValue;
			}
		});

		//And then add it to the text matches
		this.setTextMatches();
	},

	// Loop over text matches and highlight related terms
	highlightTextMatch: function (id, literal, mode) {
		var isMatch,
				firstMatch = null,
				body = this.getReview().body,
				className = this.classNames[mode];

		//console.log('highlightTextMatch', mode);

		if (mode === 'CLICK') {
			this.clearTextMatches(className);
		}

		this.textMatches.each(function (el, i) {
			isMatch = el.getAttribute('data-concept') === id || el.getAttribute('data-literal') === literal;

			if (isMatch) {
				if (el.hasCls(className)) {
				    el.removeCls(className);
				}
				else {
					el.addCls(className);

					if (!firstMatch) {
						firstMatch = el.scrollIntoView(body);
					}
				}
			}
		}, this);
	},

	// Remove the matching className
	clearTextMatches: function (className) {
		this.textMatches.each(function (el, i) {
			if (el.hasCls(className)) {
			    el.removeCls(className);
			}
		});
	},

	// Get the current document id (GUID)
	getDocumentId: function () {
		return this.application.appData.documentId;
	},

	// Set the current document id (GUID)	
	setDocumentId: function (id) {
		this.application.appData.documentId = id;
	},

	// Check save state before upload
	getSaveRequired: function () {
		return this.getController('SuggestionController').getSaveRequired();
	},

	// Get the documents for the current job
	getJobDocuments: function () {
		var store = this.getStore('DocumentStore'),
				jobLabel = this.getController('JobController').getJobLabel();

	    store.load({
	        params: {
	            jobLabel: jobLabel
	        }
	    });
	},

	// Get selected documents returns an array of Id's
	getSelectedDocuments: function () {
		var documents = [],
				grid = this.getGrid(),
				selModel = grid.getSelectionModel(),
				records = selModel.getSelection();

		Ext.Array.forEach(records, function (record, i) {
			documents.push(record.get('Id'));
		});

		return documents;
	},

	// Export documents
	exportDocuments: function () {
		var url,
				documents = this.getSelectedDocuments();

		if (documents.length) {
			url = encodeURI('./Export/Export?' + Ext.Object.toQueryString({ documents: documents }));

			window.open(url, 'Download', 'resizable=yes,scrollbars=yes,status=yes');
		}
	},

	// Delete multiple documents
	deleteDocumentSet: function () {
		var documents = this.getSelectedDocuments();

		if (documents.length) {

			Ext.Msg.confirm('Cleanup', 'All data of the selected documents will be deleted', function (button) {
				if (button === "yes") {

					Ext.Ajax.request({
						scope: this,
						url: 'Document/DeleteSet',
						params: {
							documents: documents
						},
						success: this.getJobDocuments
					});

				}
			}, this);

		}
	},

	// Delete the currently uploaded document
	deleteDocument: function () {
		var id = this.getDocumentId();

		Ext.Ajax.request({
			scope: this,
			url: 'Document/Delete',
			params: {
				id: id
			},
			success: this.reset
		});
	},

	// Reset state
	reset: function (response, options) {
		var review = this.getReview(),
				upload = this.getUpload();

		review.update(review.ownerCt.emptyText);
		upload.setRawValue('');
		upload.enable();

		this.getController('SuggestionController').reset();
	}

});