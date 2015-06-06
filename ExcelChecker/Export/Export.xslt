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
	xmlns:dataModel="http://checkers.trezorix.org/ExcelXmlChecker/Export" 
	xmlns:d2p1="http://schemas.microsoft.com/2003/10/Serialization/Arrays"
>
<xsl:template match="/">
	<xsl:processing-instruction name="mso-application">progid="Excel.Sheet"</xsl:processing-instruction>
	<Workbook xmlns="urn:schemas-microsoft-com:office:spreadsheet"
	 xmlns:o="urn:schemas-microsoft-com:office:office"
	 xmlns:x="urn:schemas-microsoft-com:office:excel"
	 xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet"
	 xmlns:html="http://www.w3.org/TR/REC-html40">
		<DocumentProperties xmlns="urn:schemas-microsoft-com:office:office">
			<Author>Excel Checker</Author>
			<LastAuthor>Excel Checker</LastAuthor>
			<Created>2011-06-14T07:40:21Z</Created>
			<LastSaved>2011-06-14T07:43:51Z</LastSaved>
			<Company>Trezorix</Company>
			<Version>14.00</Version>
		</DocumentProperties>
		<OfficeDocumentSettings xmlns="urn:schemas-microsoft-com:office:office">
			<AllowPNG/>
		</OfficeDocumentSettings>
		<ExcelWorkbook xmlns="urn:schemas-microsoft-com:office:excel">
			<WindowHeight>10290</WindowHeight>
			<WindowWidth>26835</WindowWidth>
			<WindowTopX>240</WindowTopX>
			<WindowTopY>120</WindowTopY>
			<ProtectStructure>False</ProtectStructure>
			<ProtectWindows>False</ProtectWindows>
		</ExcelWorkbook>
		<Styles>
			<Style ss:ID="Default" ss:Name="Normal">
				<Alignment ss:Vertical="Bottom"/>
				<Borders/>
				<Font ss:FontName="Calibri" x:Family="Swiss" ss:Size="11" ss:Color="#000000"/>
				<Interior/>
				<NumberFormat/>
				<Protection/>
			</Style>
		</Styles>
		<Worksheet ss:Name="Sheet1">
			<Table ss:ExpandedColumnCount="{count(dataModel:ExportModel/dataModel:Headers/*)}" ss:ExpandedRowCount="{count(dataModel:ExportModel/dataModel:Rows/dataModel:RowExportModel) + 1}" x:FullColumns="1"
			 x:FullRows="1" ss:DefaultRowHeight="15">
				<xsl:for-each select="dataModel:ExportModel/dataModel:Headers">
					<Row>
						<xsl:for-each select="*">
							<Cell>
								<Data ss:Type="String">
									<xsl:value-of select="."/>
								</Data>
							</Cell>
						</xsl:for-each>
					</Row>
				</xsl:for-each>
				<xsl:for-each select="dataModel:ExportModel/dataModel:Rows/dataModel:RowExportModel">
					<Row>
						<xsl:for-each select="*">
						<Cell>
							<Data ss:Type="String">
								<xsl:value-of select="."/>
							</Data>
						</Cell>
						</xsl:for-each>
					</Row>
				</xsl:for-each>
			</Table>
			<WorksheetOptions xmlns="urn:schemas-microsoft-com:office:excel">
				<PageSetup>
					<Header x:Margin="0.3"/>
					<Footer x:Margin="0.3"/>
					<PageMargins x:Bottom="0.75" x:Left="0.7" x:Right="0.7" x:Top="0.75"/>
				</PageSetup>
				<Selected/>
				<Panes>
					<Pane>
						<Number>3</Number>
						<ActiveRow>1</ActiveRow>
						<ActiveCol>1</ActiveCol>
					</Pane>
				</Panes>
				<ProtectObjects>False</ProtectObjects>
				<ProtectScenarios>False</ProtectScenarios>
			</WorksheetOptions>
		</Worksheet>
	</Workbook>
	</xsl:template>
</xsl:stylesheet>