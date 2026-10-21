// This file is required by karma.conf.js and loads recursively all the .spec and framework files

import 'zone.js/testing';
import { getTestBed } from '@angular/core/testing';
import {
  BrowserTestingModule,
  platformBrowserTesting
} from '@angular/platform-browser/testing';
import { setAssetPath as setIonicAssetPath } from '@ionic/core/components';
import { setAssetPath } from 'ionicons/components';

// come in main.ts: le icone vengono cercate in svg/<nome>.svg
setAssetPath(document.baseURI);
setIonicAssetPath(document.baseURI);

// First, initialize the Angular testing environment.
getTestBed().initTestEnvironment(
  BrowserTestingModule,
  platformBrowserTesting(),
);
