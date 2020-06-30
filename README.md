# Agl-Coding-Challange

Programming challenge

A json web service has been set up at the url: http://agl-developer-test.azurewebsites.net/people.json

You need to write some code to consume the json and output a list of all the cats in alphabetical order under a heading of the gender of their owner.

You can write it in any language you like. You can use any libraries/frameworks/SDKs you choose.

Example:
Male
Angel
Molly
Tigger
Female
Gizmo
Jasper
Notes
Submissions will only be accepted via github or bitbucket
Use industry best practices
Use the code to showcase your skill.

### Solution Overview

Solution consist of three projects

1. Core
Containing Domain model classes

2. Infra Structure
Containing connector for Person Pet 

3. Person Pet Process Api 

A client adapter component exposing controller to access PersonPet throung Connector. It has a Orchestrator and controller exposes function over Httpet to call orchestrator


