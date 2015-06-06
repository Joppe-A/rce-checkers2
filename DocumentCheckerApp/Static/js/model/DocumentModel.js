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

Ext.define('DC.model.DocumentModel', {
	extend: 'Ext.data.Model',

	fields: [
		{ name: 'Id', type: 'string' },
		{ name: 'SourceUri', type: 'string' },
		{ name: 'Status', type: 'string' },
		{ name: 'ModificationDate', type: 'date', dateFormat: 'j-n-Y G:i:s' }
	],

	idProperty: 'Id',

	proxy: {
		type: 'ajax',
		url: 'Document/GetByJobLabel',
		reader: 'json'
	}
});