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

Ext.define('DC.controller.WizardController', {
	extend: 'Ext.app.Controller',

	views: [
		'wizard.Step1',
		'wizard.Step2',
		'wizard.Step3'
	],

	refs: [
		{ ref: 'wizard', selector: '#wizard' },
		{ ref: 'wizardSteps', selector: '#wizardsteps' },
		{ ref: 'buttonNext', selector: '#button-next' }
	],

	init: function () {
		this.activeStep = 0;

		this.control({
			'#wizard > container': {
				'activate': this.onActivate
			},
			'#wizard button[action="next"]': {
				'click': this.nextStep
			},
			'jobgrid': {
				'itemdblclick': this.nextStep
			}
		});
	},

	// Show next wizard step
	nextStep: function (btn, e) {
		var wizard = this.getWizard(),
				layout = wizard.getLayout(),
				items = layout.getLayoutItems(),
				next = layout.getNext(),
				saveRequired = this.getController('SuggestionController').getSaveRequired();

		if (next.xtype === 'wizardstep3' && saveRequired) {
			this.confirmSave();
		}
		else {

			this.activeStep = (this.activeStep + 1 === items.length) ? 0 : this.activeStep + 1;

			layout.next({
				type: 'slide' // Animation is broken in 4.0, see: http://www.sencha.com/forum/showthread.php?130168-card-layout-but-how-to-slide-animate&p=591193
			}, true);

			this.setStepIndicator();
			this.setNextButtonText();
		}
	},

	// Confirm save
	confirmSave: function () {
		Ext.Msg.confirm('Unsaved metadata', 'Do you want to save the metadata selection?', function (button) {
			if (button === "yes") {
				this.getController('SuggestionController').saveSuggestions();
			}
			else {
				this.getController('SuggestionController').setSaveRequired(false);
			}

			this.nextStep();

		}, this);
	},

	// Private
	onActivate: function (container) {
		var xtype = container.xtype;

		switch (xtype) {
			//case 'wizardstep1':				 
			case 'wizardstep2': this.getController('DocumentController').reset();
				break;
			case 'wizardstep3': this.getController('DocumentController').getJobDocuments();
				break;
		}
	},

	// Sets the active step indicator
	setStepIndicator: function () {
		this.steps = this.steps || this.getWizardSteps().query('component');

		Ext.Array.forEach(this.steps, function (step, i) {
			step = (i === this.activeStep) ? step.addCls('step-active') : step.removeCls('step-active');
		}, this);		

		this.steps[this.activeStep].ownerCt.doLayout();
	},

	// Sets the next button's text
	setNextButtonText: function () {
		var buttonText = this.activeStep === 2 ? '<strong>Back to start </strong>' : '<strong>Next step >> </strong>';

		this.getButtonNext().setText(buttonText);
	}

});
