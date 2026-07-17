# BackupsSolution

Sistema de gestión de copias de seguridad y réplicas (*BACKUPS): motor de respaldos asíncronos, multiprotocolo y resilientes, desarrollado en **C# WinForms* con *Arquitectura Hexagonal* (Ports & Adapters).

## Descripción general

La aplicación permite programar y ejecutar respaldos de archivos de forma automática (por temporizador) o manual (por botón en la UI), comprimirlos con distintos algoritmos, fragmentarlos en volúmenes de tamaño configurable y enviarlos a un destino remoto por FTP, SFTP o SSH.

## Arquitectura

El proyecto sigue *Arquitectura Hexagonal*, separando el dominio de negocio de los detalles de infraestructura mediante puertos (Ports) y adaptadores (Adapters):


src/
├── Backups.Core/                      Núcleo de dominio y casos de uso
│   ├── Application/UseCases/          Lógica de aplicación
│   ├── Domain/Entities/               Entidades del dominio
│   ├── Domain/Services/               Servicios de dominio (verificación de cambios, logs)
│   └── Ports/                         Contratos (In/Out) que implementan los adaptadores
├── Backups.Infrastructure.Compression/  Adaptador de compresión (ZIP/RAR/LZMA + segmentación)
├── Backups.Infrastructure.Network/    Adaptador de transmisión (FTP/SFTP/SSH)
├── Backups.Adapters.Infrastructure/   Adaptadores de infraestructura adicionales
├── Backups.Adapters.UI.WinForms/      Interfaz gráfica (System Tray, logs, configuración)
├── Backups.UI/                        Formularios de configuración
├── Backups.Domain/                    Modelos de dominio compartidos
└── Backups.Ports/                     Definición de puertos compartidos

tests/
├── Backups.Domain.Tests/
├── Backups.Adapters.Tests/
└── Backups.Infrastructure.Compression.Tests/


## Grupos de trabajo por capa

| Grupo | Responsabilidad | Integrantes |
|---|---|---|
| 1 — Núcleo de Control y Orquestación | Colas, temporizadores, optimización de respaldos (validación de cambios por hash/fecha), log físico en TXT | MAMANI, FABIAN, YLLESCAS |
| 2 — Motor de Compresión y Volúmenes | Algoritmos ZIP, RAR, LZMA y fragmentación (Split/Chunking) por límite de tamaño | CHECYCHA, ESTELA, ROMERO |
| 3 — Infraestructura de Transmisión | Adaptadores de red: FTP, SFTP, SSH | MAYS, ORNETA, PONCE |
| 4 — Adaptadores de Entrada / UI WinForms | Interfaz gráfica: System Tray, logs, formulario de configuración | LUCHITO, CASTILLEJO |

El contrato de datos entre capas está definido en el archivo OpenAPI del proyecto (SolicitudRespaldo, DestinoConfig, LogRespaldo).

## Requisitos

- *Visual Studio 2022* (Community o superior)
- *.NET 10 SDK*
- Opcional, para probar compresión RAR/LZMA con la herramienta real: 7z y rar disponibles en el PATH del sistema

## Cómo abrir el proyecto

1. Clona el repositorio o descarga el .zip y descomprímelo.
2. Abre *Visual Studio* → *Archivo → Abrir → Carpeta* → selecciona la carpeta raíz del repositorio.
3. Espera a que Visual Studio restaure los paquetes NuGet automáticamente (o hazlo manualmente: clic derecho sobre cada proyecto → *Restaurar paquetes NuGet*).
4. *Compilar → Recompilar solución* para verificar que todo compila correctamente.

## Cómo ejecutar las pruebas unitarias

1. En Visual Studio: *Prueba → Explorador de pruebas* (Ctrl+E, T).
2. Clic en *Ejecutar todo* (▶️).
3. También puede ejecutarse por línea de comandos desde la raíz del repositorio:

bash
dotnet test


### Cobertura actual de pruebas — Backups.Infrastructure.Compression.Tests

- Manejo de errores: archivo de origen inexistente, algoritmo no soportado.
- Compresión ZIP: volumen único cuando el resultado cabe en el límite configurado.
- Segmentación: fragmentación correcta en múltiples volúmenes .part cuando el comprimido supera el límite de MB.
- Insensibilidad a mayúsculas/minúsculas en el nombre del algoritmo.
- RAR/LZMA: manejo de error cuando la herramienta externa (7z/rar) no está instalada en el sistema.

## Flujo de contribución

1. Cada grupo trabaja en su rama correspondiente a su capa (ej. motor-de-compresion para el Grupo 2).
2. Al finalizar, se abre un *Pull Request* hacia main.
3. El *Administrador del Repositorio* revisa y coordina la integración del código de todos los colaboradores.
