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

Ext.application({
	name: 'DC',
	autoCreateViewport: false,

	appFolder: 'Static/js',

	controllers: [
		'WizardController',
		'JobController',
		'SuggestionController',
		'DocumentController'
	],

	launch: function () {
		// Add object for storing application data
		this.appData = {};

		// Create viewport
		this.createViewport();
	},

	// Create the viewport
	createViewport: function () {

		Ext.create('Ext.container.Viewport', {
			layout: 'border',
			padding: 5,
			items: [
			/*
			{
			xtype: 'box',
			region: 'north',
			cls: 'header-app',
			html: '<a href="./">Document Checker</a> <span class="selected-job">Job: <span class="job-label"></span>'
			},
			*/

			{
				xtype: 'container',
				region: 'north',
				cls: 'header-app',
				height: 50,
				layout: {
					type: 'hbox',
					align: 'stretch'
				},
				items: [
						{
							xtype: 'box',
							width: 200,
							html: '<a href="./">Document Checker</a>'
						},
						{
							xtype: 'container',
							flex: 1,
							itemId: 'wizardsteps',
							layout: {
								type: 'hbox'
							},						
							items: [
								{ xtype: 'box', cls: 'step-indicator step-active', html: '<span class="label">1: Select job</span>' },
								{ xtype: 'box', cls: 'step-indicator', html: '<span> > </span> <span class="label">2: Check documents</span>' },
								{ xtype: 'box', cls: 'step-indicator', html: '<span> > </span> <span class="label">3: Export job</span>' }
							]							
						}
					]
				},
				{
					xtype: 'panel',
					itemId: 'wizard',
					layout: 'card',
					region: 'center',
					activeItem: 0,
					border: false,
					bodyBorder: false,
					bodyStyle: {
						background: 'none',
						padding: '5px'
					},
					items: [
						{ xtype: 'wizardstep1' },
						{ xtype: 'wizardstep2' },
						{ xtype: 'wizardstep3' }
					],
					dockedItems: [
						{
							dock: 'bottom',
							xtype: 'toolbar',
							cls: 'footer-app',							
							items: [
								{
									xtype: 'tbtext',
									html: '<span class="selected-job">Job: <span class="job-label"></span>'
								},
								'->',
								{
									xtype: 'button',
									text: '<strong>Next step >> </strong>',
									scale: 'large',
									cls: 'button-next',
									itemId: 'button-next',
									action: 'next'
								}
							]
						}
					]
				}
			]
		});
	}
});