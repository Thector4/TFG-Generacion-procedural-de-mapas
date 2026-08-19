# Generación procedural de mapas en 2D - TFG

[![Unity Version](https://img.shields.io/badge/Unity-6.0.0.18f1-blue)](https://unity.com/)
[![C#](https://img.shields.io/badge/C%23-9.0-purple)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Status](https://img.shields.io/badge/status-completed-brightgreen)](https://github.com/)

> Trabajo de Fin de Grado (TFG) para el Grado en Diseño y Desarrollo de Videojuegos  
> **Universidad San Jorge (USJ)** | Autor: **Héctor Anguita**

---

## Descripcion

Este proyecto implementa y compara dos tecnicas de **generacion procedural de mapas 2D** en Unity:

| Algoritmo | Variantes | Complejidad |
|-----------|-----------|-------------|
| **Grafos** | Prim (normal y optimizado) + Kruskal | O(V2) / O(E log V) |
| **BSP** (Binary Space Partitioning) | Subdivision recursiva con bitmasking | O(n log n) |

**Metricas evaluadas:**
- Tiempo de ejecucion
- Tiempo de CPU
- Uso de memoria

---

## Objetivos del proyecto

- Crear mapas unicos y funcionales en cada partida
- Implementar distintas tecnicas de generacion procedural
- Evaluar eficiencia y rendimiento de cada algoritmo
- Documentar el proceso de diseño y los retos tecnicos

---

## Tecnologias

- Unity 6.0.0.18f1 (2D)
- Visual Studio 2022
- C# 9.0
- Tilemaps para la representacion de mazmorras

---

## Caracteristicas

- Generacion de mapas con habitaciones conectadas mediante pasillos
- Sistema de dificultad en las salas (Inicial, Facil, Normal, Dificil, Boss)
- Decoracion procedural de habitaciones (cofres, antorchas, etc.)
- Control de parametros configurables (numero de salas, tamaño, espaciado)
- Medicion de rendimiento en tiempo real
- Codigo modular y reutilizable

---

## Estructura del proyecto

| Carpeta | Descripcion |
|---------|-------------|
| `BSP/` | Algoritmo de Particion Binaria del Espacio |
| `BSP/Scripts/` | Logica de generacion y visualizacion |
| `BSP/Tiles/` | Tiles de suelo, paredes y decoraciones |
| `BSP/_Sprites/` | Sprites del tileset |
| `Graph-Based/` | Algoritmo de Grafos (Prim, Kruskal) |
| `Graph-Based/Scripts/` | Algoritmos y generador de mazmorras |
| `Graph-Based/Prefabs/` | Prefabs de habitaciones (colores y sprites) |
| `Graph-Based/Sprites/` | Sprites para salas y pasillos |
| `Scene/` | Escenas de Unity |

---

## Algoritmos implementados

### Sistema de Grafos
- **Prim normal**: O(V2) - Ideal para pocas salas
- **Prim optimizado**: O(E log V) - Mas eficiente en conjuntos grandes
- **Kruskal**: O(E log E) - Ordenacion por peso de aristas

### BSP (Binary Space Partitioning)
- Subdivision recursiva del espacio hasta alcanzar el tamaño deseado
- Generacion de muros mediante bitmasking (8 bits para esquinas)
- Conexion automatica entre salas hermanas mediante pasillos

---

## Referencias

Partes del codigo del sistema BSP estan inspiradas en el tutorial de **Sunny Valley Studio**:
> [Unity Procedural Dungeon Generation 2D - Introduction](https://www.youtube.com/watch?v=-QOCX6SVFsk)

Tambien se ha consultado el trabajo de **Ondrej Nepozitek** sobre generacion de mazmorras basada en grafos.

---

## Documentacion completa

La memoria completa del TFG (64 paginas) esta disponible en:  
[`TFG_GeneraciónProceduralDeMapas.pdf`](./TFG_GeneraciónProceduralDeMapas.pdf)

Incluye:
- Historia de la generacion procedural en videojuegos
- Estado del arte y analisis de algoritmos
- Implementacion detallada en Unity
- Resultados y comparativas de rendimiento

---

## Resultados destacados

| Configuracion | Algoritmo | Tiempo ejecucion | Memoria |
|---------------|-----------|------------------|---------|
| 10 habitaciones | BSP | 0.39 ms | 3 KB |
| 100 habitaciones | BSP | 0.46 ms | 6 KB |
| 100 habitaciones | Grafos (Prim optimizado) | 0.78 ms | 24 KB |

> El algoritmo BSP es mas eficiente en rendimiento, pero el sistema de grafos ofrece mayor flexibilidad en el diseño de salas.

---

## Autor

**Héctor Anguita**  
Graduado en Diseño y Desarrollo de Videojuegos  
Universidad San Jorge (USJ)
