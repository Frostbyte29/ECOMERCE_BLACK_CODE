## Qué hace

## Issue

Closes #

## SOLID

- [ ] SRP — clase y su única responsabilidad:
- [ ] OCP — qué se extendió sin modificar lo existente:
- [ ] DIP — de qué interfaz depende (no de una clase concreta):

## Cómo probarlo

(pantalla y pasos)

## Checklist

- [ ] Compila y corre con F5
- [ ] Sin claves, cadenas de conexión ni secretos en el código
- [ ] Ningún controlador habla directo con AppDbContext (usa un servicio de Application)
- [ ] Ninguna vista recibe una entidad de Domain (usa un ViewModel)
- [ ] Si incluí una migración: hice pull de develop antes de crearla y es la única del PR
