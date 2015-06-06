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

Ext.define('DC.view.document.Grid', {
	extend: 'Ext.grid.Panel',
	alias: 'widget.documentgrid',

	store: 'DocumentStore',

	columns: [
		{
			header: 'Document',
			dataIndex: 'SourceUri',
			flex: 1
		},
		{
			header: 'Datum',
			dataIndex: 'ModificationDate',
			xtype: 'datecolumn',
			format: 'd-m-Y H:i:s',						
			width: 120
		}
	],

	selModel: {
		selType: 'checkboxmodel',
		mode: 'SIMPLE'
	},

	viewConfig: {
		stripeRows: true
	},

	title: 'Metadata',
	frame: true,

	buttonAlign: 'center',
	buttons: [
		{
			text: 'Export selection',
			action: 'export'
		},
		{
			text: 'Cleanup files',
			action: 'clean'
		}
	]
});