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

Ext.define('DC.view.wizard.Step1', {
	extend: 'Ext.container.Container',
	alias: 'widget.wizardstep1',

	layout: {
		type: 'hbox',
		align: 'stretch'
	},

	initComponent: function () {

		this.items = [
			{
				xtype: 'jobgrid',
				flex: 1,
				autoScroll: true
			},
			{
				xtype: 'splitter'
			},
			{
				xtype: 'container',
				itemId: 'sourcelist',
				flex: 1,
				padding: 10,
				autoScroll: true,
				html: '<p>Create a Job first.</p>',
				tpl: new Ext.XTemplate(
					'<p>Reference structures for "<strong>{Label}</strong>".</p>',
					'<ul class="skos-source-list">',
						'<tpl for="SkosSourceSelection">',
								'<li>{label}</li>',
						'</tpl>',
					'</ul>'
				)
			}
		];

		this.callParent(arguments);
	}
});