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

Ext.define('DC.view.suggestion.Add', {
	extend: 'Ext.window.Window',
	alias: 'widget.suggestionadd',

	title: 'Add candidate term',
	layout: 'fit',
	autoShow: true,
	modal: true,

	height: 200,
	minHeight: 200,
	layout: 'fit',

	initComponent: function () {

		this.items = [
      {
      	xtype: 'form',
      	width: 300,			
      	layout: {
      		type: 'vbox',
      		align: 'stretch'
      	},
      	border: false,
      	bodyPadding: 10,
      	fieldDefaults: {
      		labelAlign: 'top',
      		labelWidth: 100,
      		labelStyle: 'font-weight:bold'
      	},
      	defaults: {
      		margins: '0 0 10'
      	},
      	items: [
          {
          	xtype: 'textfield',
          	name: 'Literal',
          	fieldLabel: 'Term',
          	allowBlank: false
          },
          {
          	xtype: 'combobox',
						itemId: 'sourcecombo',
          	emptyText: 'Select...',
          	name: 'SkosSourceKey',
          	fieldLabel: 'Reference structure',
          	editable: false,          	
          	allowBlank: false,
          	queryMode: 'local',
          	store: {
          		xtype: 'store',
          		fields: ['key', 'label']
          	},
						displayField: 'label',
          	valueField: 'key'
          }
        ]
      }
    ];

		this.buttons = [
      {
      	text: 'Save',
      	action: 'save'
      },
      {
      	text: 'Cancel',
      	scope: this,
      	handler: this.close
      }
    ];

		this.callParent(arguments);
	}
});