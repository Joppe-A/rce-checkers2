<?xml version="1.0" encoding="utf-8"?>
<!-- 
  Copyright 2013 Cultural Heritage Agency of the Netherlands, Dutch National Military Museum and Trezorix bv

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.

-->
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"
	xmlns:skos="http://www.w3.org/2004/02/skos/core#" 
	xmlns:SkosTermIndexXslExtensions="ext:Term"
	exclude-result-prefixes="xsl rdf skos SkosTermIndexXslExtensions"
	>

	<xsl:param name="skos_key" />
	<xsl:variable name="root" select="/" />
	<xsl:key name="resourceUri" match="skos:Concept" use="@rdf:about" />
	
	<xsl:template match="/">
		<add>
			<xsl:for-each select="//skos:Concept">
				<xsl:variable name="broaderConceptUri" select="skos:broader[1]/@rdf:resource" /> <!-- ToDo: if no resource attr value, the broader concept may be an inner skos:Concept element of the skos:Broader element. See: http://www.ibm.com/developerworks/xml/library/x-think42/ -->
				<xsl:variable name="broaderConcept" select="key('resourceUri', $broaderConceptUri)" />
				<xsl:variable name="conceptUri" select="@rdf:about" />
				<xsl:variable name="conceptLabels" select="skos:prefLabel" />
				<xsl:for-each select="skos:prefLabel|skos:altLabel|skos:hiddenLabel|skos:indexLabel">
					<xsl:variable name="termid" >
						<xsl:value-of select="$conceptUri"/>_<xsl:value-of select="position()"/>
					</xsl:variable>
					<xsl:variable name="term" select ="." />
					<xsl:variable name="language" select="@xml:lang" />
					<xsl:variable name="conceptLabel" select="$conceptLabels[@xml:lang = $language]|$conceptLabels[1]" />
					<xsl:variable name="broaderConceptLabel" select="$broaderConcept/skos:prefLabel[@xml:lang = $language]|$broaderConcept/skos:prefLabel[1]" />
					<xsl:variable name="source" select="name()" />
					<!-- public XPathNavigator Term(string termId, string term, string language, string source) -->
					<xsl:variable name="termIndex" select="SkosTermIndexXslExtensions:Term($term, $language, $source)/root" />
					<doc>
						<field name="doctype">full</field>
						<field name="id">
							<xsl:value-of select="$termid"/>
						</field>
						<field name="skos_concept">
							<xsl:value-of select="$conceptUri"/>
						</field>
						<field name="skos_concept_label">
							<xsl:value-of select="$conceptLabel"/>
						</field>
						<field name="skos_broader">
							<xsl:value-of select="$broaderConceptUri"/>
						</field>
						<field name="skos_broader_label">
							<xsl:value-of select="$broaderConceptLabel"/>
						</field>
						<field name="skos_key">
							<xsl:value-of select="$skos_key"/>
						</field>
						<field name="type">ConceptTerm</field>
						<field name="source">
							<xsl:value-of select="$source"/>
						</field>
						<field name="xml_lang"><xsl:value-of select="$language"/></field>
						<field name="literal_form">
								<xsl:value-of select="."/>
						</field>
						<field name="term">
							<xsl:value-of select="$termIndex/full/term" />
						</field>
						<field name="full_term">
							<xsl:value-of select="$termIndex/full/term" />
						</field>
					</doc>
					<xsl:for-each select="$termIndex/termenrichment">
						<doc>
							<field name="doctype">full</field>
							<field name="id">
								<xsl:value-of select="$termid"/>_<xsl:value-of select="dictionarycollection" />_<xsl:value-of select="wordgroup" />_<xsl:value-of select="position()"/>
							</field>
							<field name="skos_key">
								<xsl:value-of select="$skos_key"/>
							</field>
							<field name="skos_concept">
								<xsl:value-of select="$conceptUri"/>
							</field>
							<field name="skos_concept_label">
								<xsl:value-of select="$conceptLabel"/>
							</field>
							<field name="skos_broader">
								<xsl:value-of select="$broaderConceptUri"/>
							</field>
							<field name="skos_broader_label">
								<xsl:value-of select="$broaderConceptLabel"/>
							</field>
							<field name="type">TermEnrichment</field>
							<field name="source">
								<xsl:value-of select="$source"/>
							</field>
							<field name="xml_lang">
								<xsl:value-of select="$language"/>
							</field>
							<field name="literal_form">
								<xsl:value-of select="."/>
							</field>
							<field name="term">
								<xsl:value-of select="enrichedterm" />
							</field>
							<field name="full_term">
								<xsl:value-of select="enrichedterm" />
							</field>
							<field name="dictionarycollection">
								<xsl:value-of select="dictionarycollection" />
							</field>
							<field name="wordgroup">
								<xsl:value-of select="wordgroup" />
							</field>
						</doc>
					</xsl:for-each>
					<xsl:for-each select="$termIndex/partial">
						<doc>
							<field name="id">
								<xsl:value-of select="$source"/>_<xsl:value-of select="$language"/>_<xsl:value-of select="partialterm"/>
							</field>
							<field name="doctype">partial</field>
							<field name="term">
								<xsl:value-of select="partialterm"/>
							</field>
							<field name="type">PartialTerm</field>
							<field name="source">
								<xsl:value-of select="$source"/>
							</field>
							<field name="xml_lang">
								<xsl:value-of select="$language"/>
							</field>
						</doc>
					</xsl:for-each>
				</xsl:for-each>
			</xsl:for-each>
		</add>
	</xsl:template>
	
</xsl:stylesheet>