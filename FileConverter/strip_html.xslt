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
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xhtml="http://www.w3.org/1999/xhtml">
  
  <xsl:output method="text" indent="no"/>
  <xsl:strip-space elements="*"/>

  <xsl:variable name="crLf" select="'&#013;&#010;'" />
  
  <xsl:template match="/">
    <xsl:value-of select="xhtml:html/xhtml:head/xhtml:title" />
    <xsl:value-of select="$crLf"/>
    <xsl:call-template name="RenderBody">
      <xsl:with-param name="select" select="xhtml:html/xhtml:body" />
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="RenderBody">
    <xsl:param name="select" />
    <xsl:call-template name="Copy">
      <xsl:with-param name="select" select="$select" />
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="Copy">
    <xsl:param name="select" />

    <xsl:for-each select="$select">
     <!--name: <xsl:value-of select="name()"/>-->
      <xsl:choose>
        <!-- element catches -->
        <xsl:when test="name()='p' or name()='div'">
          <xsl:if test="normalize-space(.) != ''">
            <xsl:call-template name="Copy">
              <xsl:with-param name="select" select="*|text()" />
            </xsl:call-template>
            <xsl:value-of select="$crLf"/>
          </xsl:if>
        </xsl:when>
        <xsl:otherwise>
          <xsl:copy>
            <xsl:for-each select="@*">
              <xsl:variable name="name" select="name()" />
              <xsl:variable name="value" select="." />
              <xsl:choose>
                <!-- attribute catches -->
                <xsl:when test="$name='xxx'">
                </xsl:when>
                <xsl:otherwise>
                  <xsl:copy/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:for-each>
            <xsl:call-template name="Copy">
              <xsl:with-param name="select" select="*|text()" />
            </xsl:call-template>
          </xsl:copy>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>
  </xsl:template>    
  
</xsl:stylesheet>
