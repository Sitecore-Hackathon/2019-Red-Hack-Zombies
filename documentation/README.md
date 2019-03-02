# Documentation

The documentation for this years Hackathon must be provided as a readme in Markdown format as part of your submission. 

You can find a very good reference to Github flavoured markdown reference in [this cheatsheet](https://github.com/adam-p/markdown-here/wiki/Markdown-Cheatsheet). If you want something a bit more WYSIWYG for editing then could use [StackEdit](https://stackedit.io/app) which provides a more user friendly interface for generating the Markdown code. Those of you who are [VS Code fans](https://code.visualstudio.com/docs/languages/markdown#_markdown-preview) can edit/preview directly in that interface too.

Examples of things to include are the following.

## Summary

**Category:** Best enhancement to the Sitecore Admin (XP) UI for Content Editors & Marketers

What is the purpose of your module? What problem does it solve and how does it do that?

-Often Content authors create content items and they want to make them clone of some other content item. However as per current sitecore 
functionality one can only create a new clone of an item but they can't convert an existing item clone of some other item.

-This is when this module comes into picture. On installtion of this module a new button called "CloneMe" is added to the Review tab in 
Sitecore ribbon. On click of this button a pop up will be shown to the content authors where they can select a source item, 
post which the current item will be made clone of the selected source item.

## Pre-requisites

Does your module rely on other Sitecore modules or frameworks?

- No

## Installation

Provide detailed instructions on how to install the module, and include screenshots where necessary.

-Please have Sitecore 9.1 with .Net Framework 4.7.1 installed on your machine.
-Upload and install the CloneMe-1.0.zip package in your Sitecore instance.
-Now, you are good to go.

## Configuration

How do you configure your module once it is installed? Are there items that need to be updated with settings, or maybe config files need to have keys updated?

-No additional configuration is required. Once the module is installed, you can start using it instantly.

## Usage

step 1. Open your Sitecore 9.1 instance and navigate to Content Editor.
step 2. Go to any existing item which you want to make as a cloned item.
step 3. Navigate to the Review tab. You see a "CloneMe" button. Please refer to below screenshot,

![CloneMe Button](images/cloneme-button.png?raw=true "CloneMe Button")

step 4. Click on the "CloneMe" button. Then a pop up will be shown.
step 5. Select the source item from which you want to clone the current item and click OK. Please refer to below screenshot,

![select source item](images/select-source-item?raw=true "Select Source Item")

After few seconds a pop up will be shown which will contain the success message. Please refer to below screenshot

![Clone Success message](images/clone-success-message.png?raw=true "Clone Success Message")

## Video

Please provide a video highlighing your Hackathon module submission and provide a link to the video. Either a [direct link](https://www.youtube.com/watch?v=EpNhxW4pNKk) to the video, upload it to this documentation folder or maybe upload it to Youtube...

[![Sitecore Hackathon Video Embedding Alt Text](https://img.youtube.com/vi/EpNhxW4pNKk/0.jpg)](https://www.youtube.com/watch?v=EpNhxW4pNKk)
