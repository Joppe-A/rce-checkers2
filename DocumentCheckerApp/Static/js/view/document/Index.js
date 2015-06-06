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

Ext.define('DC.view.document.Index', {
	extend: 'Ext.form.Panel',
	alias: 'widget.documentindex',

	layout: {
		type: 'vbox',
		align: 'stretch'
	},

	initComponent: function () {

		this.emptyText = '<p class="empty">Enter an url or upload a document first.</p>';

		this.items = [
			{
				xtype: 'panel',
				itemId: 'review',
				html: this.emptyText,
				flex: 1,
				padding: 10,
				border: false,
				autoScroll: true
			}
		];

		this.dockedItems = [
			{
				dock: 'top',
				xtype: 'toolbar',
				items: [
					{
						xtype: 'textfield',
						itemId: 'url',
						name: 'url',
						width: 300,
						emptyText: 'http://',
						selectOnFocus: true,
						fieldLabel: 'Url',
						labelWidth: 25,
						labelAlign: 'right',
						labelStyle: 'font-weight:bold',
						listeners: {
							'change': function () {
								var button = this.next('button');
								if (this.getValue() !== '') {
								    button.enable();
								} else {
									button.disable();
								}
							}
						}
					},
					{
						xtype: 'button',
						action: 'check-url',
						disabled: true,
						text: 'Check url'
					},
					{
						xtype: 'filefield',
						itemId: 'upload',
						name: 'file',
						width: 450,
						//allowBlank: false,
						buttonText: 'Select Document',
						fieldLabel: 'Or document',
						labelAlign: 'right',
						labelStyle: 'font-weight:bold'
					}
				]
			}
		];

		this.callParent(arguments);
	}
});