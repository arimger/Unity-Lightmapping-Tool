# Lightmapping Tool

## Introduction

This repository contains **experimental** tool for blending and switching lightmaps.

Features:
- serializing lightmaps into dedicated SO files
- switching lightmaps in runtime and in the Edit mode
- blending lightmaps in runtime and in the Edit mode (blend multiple lightmaps and particular texture indexes)
- loading lightmaps from a directory

Problems to solve:
- ReflectionProbes blending is disabled 
- high memory usage from runtime textures
- initialization spread over time

## System Requirements
Unity 2019.1 or newer

## Demonstration

[![demonstration](https://img.youtube.com/vi/Nj0vsYJFZqY/0.jpg)](https://www.youtube.com/watch?v=Nj0vsYJFZqY)