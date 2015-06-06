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

Ext.define('DC.controller.JobController', {
	extend: 'Ext.app.Controller',

	models: ['JobModel', 'StructureModel', 'DocumentModel'],
	stores: ['JobStore', 'StructureStore', 'DocumentStore'],

	views: [
		'job.Grid',		
		'job.Add'
	],

	refs: [
		{ ref: 'grid', selector: 'jobgrid' },
		{ ref: 'sourceList', selector: '#sourcelist' }
	],

	init: function () {		
		this.getStore('JobStore').on('load', this.onJobStoreLoad, this);

		this.control({
			'jobgrid': {
				'selectionchange': this.onSelectionChange
			},
			'jobgrid button[action="add"]': {
				'click': this.addJob
			},
			'jobadd #structuregroup': {
				'beforerender': this.addStructureCheckboxes
			},
			'jobadd button[action="save"]': {
				'click': this.saveJob
			}
		});
	},

	// Private
	// Give feedback when checker is not available, otherwise select first record
	onJobStoreLoad: function (store, records, success) {
		if (!success) {
			Ext.Msg.alert('Error', 'Document checker is not available. Contact us at <a href="mailto:helpdesk@trezorix.nl?subject=Document Checker error">helpdesk@trezorix.nl</a>');
		}
		else if (records.length) {
			this.getGrid().getSelectionModel().select(0);
		}
	},

	// Add structure checkboxes
	addStructureCheckboxes: function (fieldset) {
		var checkboxes = [],
				structureStore = this.getStore('StructureStore');

		structureStore.each(function (record) {
			checkboxes.push({
				boxLabel: record.get('label'),
				inputValue: record.get('key'),
				name: 'skosSourceSelection'
			})
		});

		fieldset.add(checkboxes);
	},

	onSelectionChange: function (sm, records) {
		var label,
				record = records[0];

		if (record) {
			label = record.get('Label');

			this.setJobLabel(label);

			this.updateSkosSourceList(record);
		}
	},

	// Update panel to show selected SKOS sources.	
	updateSkosSourceList: function (record) {
		var data = record.raw,
				sources = data.SkosSourceSelection,
				sourceList = this.getSourceList();

		sourceList.update(data);
		this.setJobSkosSources(sources);
	},

	// Get the job's skosSources
	getJobSkosSources: function () {
		return this.application.appData.jobSkosSources;
	},

	// Set the job's skosSources
	setJobSkosSources: function (sources) {
		this.application.appData.jobSkosSources = sources;
	},

	// Get the selected job
	getJobLabel: function () {
		return this.application.appData.jobLabel;
	},

	// Set the selected job
	setJobLabel: function (label) {
		this.application.appData.jobLabel = label;

		Ext.select('.job-label').update(label);
	},

	// Show the add job view
	addJob: function (button, e) {
		var view = Ext.widget('jobadd');
	},

	// Save and select the job
	saveJob: function (button, e) {
		var window = button.up('window'),
        form = window.down('form').getForm(),
				grid = this.getGrid(),
				store = grid.getStore(),
				selModel = grid.getSelectionModel();

		if (form.isValid()) {
			form.submit({
				scope: this,
				url: 'Job/Add',
				failure: function (form, action) {
					Ext.Msg.alert('Error', action.response.statusText);
				},
				success: function (form, action) {
					var record,
							values = form.getValues(),
							label = values.label;

					window.close();

					store.load(function (records, operation, success) {
						record = store.findRecord('Label', label);

						selModel.select(record);
					});
				}
			});
		}
	}

});
