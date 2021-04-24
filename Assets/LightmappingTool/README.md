# Lightmapping Tool

## Introduction

This repository contains **experimental** tool for blending and switching lightmaps.

Features:
- switching lightmaps in runtime and in the Edit mode
- blending lightmaps in runtime and in the Edit mode (possibility to blend multiple lightmaps and particular texture indexes)
- serializing lightmaps in dedicated SO files
- loading lightmaps from directory

Problems to solve:
- ReflectionProbes blending is disabled 
- high memory usage from runtime textures
- initialization spread over time

## System Requirements
Unity 2019.1 or newer

## Demonstration

https://www.youtube.com/watch?v=Nj0vsYJFZqY&ab_channel=Mi%C5%82oszMatkowski