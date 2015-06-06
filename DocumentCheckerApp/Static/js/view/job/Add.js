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

Ext.define('DC.view.job.Add', {
	extend: 'Ext.window.Window',
	alias: 'widget.jobadd',

	title: 'Add job',
	layout: 'fit',
	autoShow: true,
	modal: true,
	autoScroll: true,
	width: 500,
	height: 400,

	initComponent: function () {
		this.items = [
      {
      	xtype: 'form',      	  
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
          	name: 'label',
          	fieldLabel: 'Job name',
          	allowBlank: false
          },
					{
						xtype: 'checkboxgroup',
						fieldLabel: 'Select reference structures for this job',
						autoScroll: true,
						itemId: 'structuregroup',
						columns: 2,
						vertical: true,
						allowBlank: false
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