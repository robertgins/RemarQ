It’s safe to say that Microsoft SharePoint Server™ is one of the more
widely deployed technologies for document management and business
collaboration. *RemarQ* ™ is an enhancement to SharePoint that provides
visual tracking for what you (as a SharePoint user) do on a SharePoint
site. While SharePoint has the “**New**” indicator for content on a
site, in general it is difficult for users to identify which content
they have already read. This is especially true for large document
libraries and discussion threads. *RemarQ* keeps track of all of the
things that you read in SharePoint and makes sure you know, at a glance,
what you have or have not read. We are all familiar with email, and
usually email systems do something similar in our Inbox. Messages that
are unread usually are displayed with some type of visual clue (**bold**
or *highlighted*) that indicates whether or not you have read it.
*RemarQ* does the same thing for SharePoint but with lots of bells and
whistles. The first release of *RemarQ* supported Microsoft SharePoint
2013 Foundation and Server editions. This is the 2.0 release and which
also supports SharePoint 2016, and is now an open source project hosted
here on GitHub.

*RemarQ* started life as an exercise in how to provide customization
features for SharePoint 2003, that version never became publically
available. The versions for SharePoint 2007 and 2010 were provided as
open source CodePlex projects (<http://readunreadcolumn.codeplex.com/> &
<http://voodoo.codeplex.com/> ). In 2013 I decided that it might be
economically worthwhile to turn the technology into a licensed product
and sell it. Unfortunately, *RemarQ* was never a financial success 
so I decided to repackage it as an open source project and release
it here.

*RemarQ* is a SharePoint farm solution, it’s not a SharePoint App. It
does its work by integrating a tracking database with dynamically
generated JSLink scripts. *RemarQ* activates when a well-known site
column (named RemarQ) is added to a list. The addition of the column
queues up the list to be modified by a SharePoint job. The job updates
the list schema, views on the list, context menu items, event handlers
for the list, and then processes all existing items in the list. The
reverse work is done when the site column is removed. The visual
indicators for the list are processed in the browser in JavaScript that
is generated from the JSLink attributes for the list. Items are marked
read by several ways, you can use the context menus, you can read the
item and you can also install an http module that will monitor for
direct access from Office applications. The read marks are stored in an
external database

A couple of key things about the solution files and the conversion to
open source:

-   The initial release version of *RemarQ* leveraged the licensed
    version of the Kendo UI, this release uses the open source Kendo
    version for one of the more complicated reports (the per document
    readers report) but otherwise Kendo is no longer used in
    the project.

-   In the initial release of *RemarQ*, the read marks tables were
    available as external data sources (for CAML query), that feature is
    not available in this version of *RemarQ*.

-   All licensing has been removed from the project and all of the
    features of the enterprise version of *RemarQ* are part of
    this project.

-   The technology works on both SharePoint 2013 and 2016, the
    development environment uses the SharePoint 2013 Office Developer
    for Visual Studio 2013. I have **NOT** yet migrated the project to 
    Visual Studio 2015. The installation package deploys and
    operates on both SharePoint 2013 and 2016.  

-   The project ships in US English, several alternate languages
    are available. The Balsamic Solutions culture manager can be used to
    update an installation but is not included in this distribution. If
    you need an alternate language, please send a request to
    <info@balsamicsolutions.com> for access to the culture manger and
    the language files for *RemarQ*.

-   The documentation has been updated to reflect the current feature
    set and describes how the technology is installed and managed.

-   The signing key used for this project is the same one as used for
    the release version. The logic is that anyone who has the product
    installed may want to reuse the lists they have so the key has
    been included.

-   The repository includes a Setup project that requires InstallShield
    Express 2015 (the free version). The output from this project is the 
    installer described in the administration guide. It will install
    *RemarQ* and all supporting files on a SharePoint server.

This repository has a number of directories including the base solution and some 
supporting projects and files:
-   *BalsamicSolutions.ReadUnreadSiteColumn* : A Visual Studio SharePoint
    project that is the *RemarQ* solution
-   *DebugConsole* : A Visual Studio project that can load test data and setup
    different test and code evaluation scenairos
-   *Documents* : Installation and administration documentation  
-   *Images* : Graphics related to the project           
-   *Other* :  Files needed by the instllation routines or related to the project. 
-   *RemarQListProcessor* : A Visual Studio project that is a windows service that can
    replace the SharePoint Jobs with an external processor
-   *RemarQSetup* : An InstallShield Express setup project
-   *BuildTools* : This project is included in the Setup solution so that the pre and
    post build events can be leveraged to create the WSP file for the setup project
    otherwise it provides no value. 
-   *Translations* : Some language files
-   *Utilities* : A Visual Studio project with a utilities commmand processor
    for fixing common installation and configuration problems
-   *WSP* : A working directory that contains the output WSP solution file. An MSI 
    installer for the solution is also stored here. Due to the limitations of the
    Express version of InstallShield the MSI file is in a subdirectory named
    *Express\SingleImage\DiskImages\DISK1*. If you just want to use the technology
    then this might be the best option for you
-   *BalsamicSolutions.ReadUnreadSiteColumn.Setup.sln* : A Visual Studio solution file
-   *BalsamicSolutions.ReadUnreadSiteColumn.sln* : A Visual Studio solution file