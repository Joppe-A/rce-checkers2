Checkers 2
==========

Introduction
------------
Checkers 2 is a document analyzer. It scans documents for occurrences of SKOS
concepts.

The analysis result can be examined with a small editor. Within the editor
concepts can be approved and then exported to an Excel file. Documents can be
uploaded or passed to the analyzer through an URL. Documents can be grouped in a
*job*.

SKOS (source) documents can be uploaded to a SKOS repository. Profiles can be
formed to bundle multiple SKOS sources for analysis.

Checkers 2 was developed by [Trezorix](http://www.trezorix.nl), first quarter of 2010, for the Cultural Heritage
Agency of the Netherlands and the Dutch National Military Museum.


About the code
--------------
The source code distribution is a provided as is, no documentation is currently
available.

-   The analyzer requires an Apache SOLR instance, with the schema that is
    included with this distribution.
-   SKOS sources can be uploaded through the provided *ConfigurationManager
    *application.
-   No database system is required, all storage is file based.
-   Documents undergo XHTML conversion through Apache Tika before analysis.

Known issues
------------
-   Jobs with many analysis results may exceed maximum URL length while
    attempting to export.


