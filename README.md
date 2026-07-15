# Papelería DB — Escritorio

Cliente WPF independiente que consume la API REST de Papelería DB.

## Configuración por computadora

Cada computadora debe tener una caja asignada mediante una variable de entorno:

```text
PAPELERIA_CAJA_ID=1
```

El valor puede ser cualquier número positivo existente en el backend. No hay un
límite de tres cajas; para agregar más computadoras se crean nuevas cajas y se
asigna el identificador correspondiente a cada equipo.

Opcionalmente se puede asignar un nombre estable a la terminal:

```text
PAPELERIA_TERMINAL_ID=CAJA-MOSTRADOR-01
```

Si no se configura, se utiliza el nombre de Windows de la computadora. Este
identificador deja preparada la aplicación para auditoría y sincronización
local en una fase posterior.

Para utilizar otra instalación de la API:

```text
PAPELERIA_API_URL=https://servidor.example.com/
```

Si no se especifica, se utiliza el backend público configurado para el proyecto.

## Compilación

```powershell
dotnet build PapeleriaDB.csproj
```
