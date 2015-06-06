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

Ext.define('DC.view.suggestion.Grid', {
	extend: 'Ext.grid.Panel',
	alias: 'widget.suggestiongrid',

	store: 'SuggestionStore',

	columns: [
		{
			header: 'Term',
			dataIndex: 'Literal',
			//flex: 1
			width: 110
		},
		{
			xtype: 'templatecolumn',
			tpl: '<span data-qtip="{SkosSourceKey}">{SkosSourceLabel}</span>',
			header: 'Reference structure',
			dataIndex: 'SkosSourceKey',
			flex: 1
		},
		{
			header: 'Broader concept',
			dataIndex: 'BroaderLiteral',
			flex: 1
		}
	],

	selModel: {
		selType: 'checkboxmodel',
		mode: 'SIMPLE',
		checkOnly: true //only select item by clicking the checkbox
	},

	viewConfig: {
		stripeRows: true,
		deferEmptyText: true,
		emptyText: '<p class="empty-text empty-suggestions">No metadata suggestions found</p>'
	},

	title: 'Metadata suggestions',
	frame: true,
	cls: 'suggestion-grid',


	buttonAlign: 'center',
	fbar: [
		{
			xtype: 'tbtext',
			itemId: 'success',
			hidden: true,
			cls: 'save-success',
			text: '<span>Save success!</span>'
		},
		{
			xtype: 'button',
			text: 'Save',
			action: 'save',
			disabled: true
		},
		{
			xtype: 'button',
			text: 'Add term',
			action: 'add',
			disabled: true
		}
	]

	/*
	buttonAlign: 'center',
	buttons: [
		{
			text: 'Save',
			action: 'save',
			disabled: true
		},
		{
			text: 'Add term',
			action: 'add',
			disabled: true
		}
	]
	*/

});