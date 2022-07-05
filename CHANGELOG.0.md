# Changelog

## 0.0.1

May  6, 2017

* Basically functional.

## 0.1.1

Nov 21, 2017

* Rewrite the back logic
* Fix issue with sorting
* Fix file reading problem

## 0.1.2

Nov 22, 2017

* Rewrite the file reading part. Now it is asynced. (Very great thanks to TautCony)
* Add detection for streams with hugely different duration. (Background color is PaleVioletRed)
* Add detection for strange chapters. (Background color is Yellow)

## 0.1.3

Nov 23, 2017

* Can bypass files that have already been loaded. (Great thanks to TautCony)
* Fix issue with duration detect.
* Auto generated version number.

## 0.1.4

Jan 20, 2018

* Now multiple selection is enabled.
* Add first-chapter-not-start-at-0 detection.
* Resovle an issue that causes program crash when loading non-Matroska file.

## 0.1.5

Jan 19, 2019

* New technical information window for pros.
* Add delay detection.
* Add filename-content mismatch detection (super lazy, use with caution).
* Show VFR video as VFR rather than average framerate.

## 0.1.6

Apr 5, 2022

* Add framerate color
* Fix fps color rendering problem
* Extend data structure length to avoid data overflow
* Mitigate chapter name parsing issue
