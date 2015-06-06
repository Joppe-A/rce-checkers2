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

Ext.define('DC.controller.SuggestionController', {
	extend: 'Ext.app.Controller',

	models: ['SuggestionModel'],
	stores: ['SuggestionStore'],

	views: [
		'suggestion.Grid',
		'suggestion.Add'
	],

	refs: [
		{ ref: 'grid', selector: 'suggestiongrid' }
	],

	init: function () {
		this.control({
			'suggestiongrid': {
				//'itemmouseenter': this.toggleMouseOver,
				//'itemmouseleave': this.toggleMouseOver,
				'beforeitemmousedown': this.onItemClick,
				'select': this.toggleSelection,
				'deselect': this.toggleSelection,
				'selectionchange': this.onSelectionChange
			},
			'suggestiongrid button[action="add"]': {
				'click': this.addCandidateTerm
			},
			'suggestionadd #sourcecombo': {
				'afterrender': this.setJobSkosSourceData
			},
			'suggestionadd button[action="save"]': {
				'click': this.saveCandidateTerm
			},
			'suggestiongrid button[action="save"]': {
				'click': this.saveSuggestions
			}
		});

	},

	// Load suggestions into the store
	getSuggestions: function (id) {
		var store = this.getGrid().getStore();

		store.load({
			params: {
				id: id
			},
			scope: this,
			callback: this.handleLoad
		});
	},

	// Handle response from load action
	handleLoad: function (records, operation, success) {
		this.disableButtons(false);
	},

	// Load the Job's skos sources into the combobox's store
	setJobSkosSourceData: function (combo) {
		var data = this.getController('JobController').getJobSkosSources();

		combo.store.loadData(data, false);
	},

	// Shows the 'Add candidate term view'
	addCandidateTerm: function (button, e) {
		var view = Ext.widget('suggestionadd');
	},

	// Add the candidate term to the store and selects it.
	saveCandidateTerm: function (button, e) {
		var record,
				window = button.up('window'),
				formPanel = window.down('form'),
        form = formPanel.getForm(),
				values = form.getValues(),
				combo = formPanel.down('combobox'),
				grid = this.getGrid(),
				store = grid.getStore(),
				selModel = grid.getSelectionModel();

		values['SkosSourceLabel'] = combo.getRawValue();

		if (form.isValid()) {
			record = store.add(values)[0];
			window.close();

			// Match in document
			this.getController('DocumentController').addCandidateTextMatch(record.get('Literal'));

			// Select term
			selModel.select(record, true);
		}
	},

	// Private
	onItemClick: function (view, record, node, index, e) {
		var id = record.get('Id'),
				literal = record.get('Literal');

		Ext.fly(node).radioCls('x-grid-row-clicked');
		this.getController('DocumentController').highlightTextMatch(id, literal, 'CLICK');
	},

	// Toggle highlight by mouseenter/leave
	toggleMouseOver: function (view, record, item, index, e) {
		var id = record.get('Id'),
				literal = record.get('Literal');

		this.getController('DocumentController').highlightTextMatch(id, literal, 'HOVER');
	},

	// Toggle highlight by (de)select
	toggleSelection: function (selModel, record, index) {
		var id = record.get('Id'),
				literal = record.get('Literal');

		this.getController('DocumentController').highlightTextMatch(id, literal, 'SELECT');
	},

	// Get data from the selected suggestions
	getSelectedSuggestionsData: function () {
		var data = [],
				grid = this.getGrid(),
				selModel = grid.getSelectionModel(),
				records = selModel.getSelection();

		Ext.Array.forEach(records, function (record, i) {
			data.push(record.data);
		});

		return data;
	},

	// Posts suggestions to the server.
	saveSuggestions: function (button) {
		var data = this.getSelectedSuggestionsData(),
				documentId = this.getController('DocumentController').getDocumentId();

		//button.addCls('saving').setText('Saving...');

		Ext.Ajax.request({
			scope: this,
			url: 'Review/SaveReviewResult/',
			jsonData: data,
			params: {
				id: documentId // Params will be appended to the url, because we're using jsonData!
			},
			callback: this.handleSave
		});
	},

	// Handle response from save action
	handleSave: function (options, success, response) {
		var successMsg = this.getGrid().down('#success');

		this.setSaveRequired(!success);

		if (!success) {
			Ext.Msg.alert('Error', 'Error saving suggestion, please try again.');
		}
		else {
			successMsg.show();

			setTimeout(function () {
				successMsg.hide();
			}, 1000);
		}
	},

	// Handle 'selectionchange' event
	onSelectionChange: function (selModel, records) {
		if (records.length) {
			this.setSaveRequired(true);
		}
	},

	// Set 'save required' state
	setSaveRequired: function (required) {
		this.saveRequired = required;

		this.setSaveButtonState(required);
	},

	// Set the save button state
	setSaveButtonState: function (required) {
		var save = this.getGrid().down('button[action="save"]');

		if (required) {
			save.addCls('save-required').enable();
		}
		else {
			save.removeCls('save-required').disable();
		}
	},

	// Disable skip and add buttons
	disableButtons: function (disable) {
		var buttons = this.getGrid().query('button');

		Ext.Array.forEach(buttons, function (button, i) {
			if (button.action !== 'save') {
				button.setDisabled(disable);
			}
		});
	},

	// Get 'save required' state and show confirm when save is required
	getSaveRequired: function (callback, scope) {
	    var required = this.saveRequired || false;

		return required;
	},

	// Reset state
	reset: function () {
		var grid = this.getGrid(),
				store = grid.getStore();

		this.disableButtons(true);
		store.removeAll();
		this.setSaveRequired(false);
	}

});
