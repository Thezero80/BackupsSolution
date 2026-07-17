# BackupsSolution

Sistema de Gestión de Copias de Seguridad y Réplicas — motor de respaldos asíncronos, multi-protocolo y resiliente en C# .NET 10.0 WinForms.

Arquitectura **Hexagonal (Ports & Adapters)** con separación por grupos de trabajo.

---

## Estado actual

| Grupo | Responsabilidad | Estado |
|---|---|---|
| **Grupo 1** — Núcleo de Control | Orquestación, validación de cambios (SHA256), log de estados | ✅ Funcional |
| **Grupo 2** — Compresión | ZIP/RAR/LZMA + segmentación por volumen | ✅ Funcional |
| **Grupo 3** — Transmisión | FTP/SFTP/SSH con FluentFTP y SSH.NET | ✅ Funcional |
| **Grupo 4** — UI WinForms | Formulario de configuración y envío | ⚠️ Parcial |
| **Pruebas** | Suite de 27 pruebas para Grupo 1 | ✅ Todas pasan |

---

## Estructura del proyecto

```
BackupsSolution/
├── BackupsSolution.slnx
├── doc/
│   ├── RFS.txt                          Requerimientos funcionales (RF2, RF3, RF4)
│   └── contratoSDD.yaml                 Contrato OpenAPI 3.0.3
│
├── src/
│   ├── Backups.Core/                    Núcleo del sistema (Grupo 1)
│   │   ├── Domain/Entities/
│   │   │   ├── SolicitudRespaldo.cs     DTO de entrada (solicitud de respaldo)
│   │   │   └── RegistroRespaldo.cs      DTO de salida (registro en historial)
│   │   ├── Domain/Services/
│   │   │   ├── VerificadorCambios.cs    Cálculo SHA256 y comparación de hashes
│   │   │   └── GestorLog.cs            Persistencia JSON de registros
│   │   ├── Application/UseCases/
│   │   │   └── EjecutarRespaldoUseCase.cs  Orquestador: verificar→comprimir→transmitir
│   │   └── Ports/
│   │       ├── In/IEjecutarRespaldoUseCase.cs   Puerto de entrada
│   │       └── Out/
│   │           ├── ICompressionService.cs        Puerto de salida (Grupo 2)
│   │           ├── ITransmissionService.cs       Puerto de salida (Grupo 3)
│   │           └── IHistorialRespaldoService.cs  Puerto de salida (historial)
│   │
│   ├── Backups.Infrastructure.Compression/   Grupo 2
│   │   └── CompressionAdapter.cs        ZIP nativo, LZMA/RAR via CLI, segmentación
│   │
│   ├── Backups.Infrastructure.Network/       Grupo 3
│   │   ├── NetworkTransmissionAdapter.cs FTP/SFTP/SSH con FluentFTP y SSH.NET
│   │   └── destinos.json                Config de destinos de ejemplo
│   │
│   ├── Backups.UI/                           UI funcional integrada
│   │   ├── Program.cs                    Entry point → FormConfig
│   │   └── FormConfig.cs                Formulario completo: selección, envío, historial
│   │
│   ├── Backups.Adapters.UI.WinForms/        Grupo 4 (esqueleto)
│   │   ├── Form1.cs                     Formulario vacío sin lógica
│   │   └── Form1.Designer.cs            Diseñador con menú y controles
│   │
│   ├── Backups.Domain/                       Vacío (solo .csproj)
│   ├── Backups.Ports/                        Vacío (solo .csproj)
│   └── Backups.Adapters.Infrastructure/     Vacío (solo .csproj)
│
├── tests/
│   ├── Backups.Domain.Tests/              xUnit — sin tests escritos
│   └── Backups.Adapters.Tests/            xUnit — sin tests escritos
│
└── control-nucleo-test/                   Pruebas del Grupo 1
    ├── Backups.Core.Tests.csproj
    ├── Program.cs                        27 pruebas con mocks
    ├── README.md                         Documentación de pruebas
    └── expected/                         Datos de referencia
```

---

## Flujo del sistema

```
Usuario (UI) → EjecutarRespaldoUseCase
                    │
                    ├── VerificadorCambios.CalcularHashArchivo()
                    ├── Comparar con último hash confirmado
                    │
                    ├── Si CAMBIÓ:
                    │     ├── GestorLog → "PROCESANDO"
                    │     ├── ICompressionService → comprimir + segmentar (Grupo 2)
                    │     ├── ITransmissionService → enviar por red (Grupo 3)
                    │     └── GestorLog → "COMPLETADO" o "FALLIDO"
                    │
                    └── Si NO cambió:
                          └── GestorLog → "OMITIDO_SIN_CAMBIOS"
```

---

## Estados del respaldo

| Estado | Descripción |
|---|---|
| `OMITIDO_SIN_CAMBIOS` | Archivo sin modificaciones desde el último respaldo |
| `PROCESANDO` | Iniciando respaldo |
| `COMPRIMIENDO_VOLUMENES` | Grupo 2 comprimiendo archivos |
| `ENVIANDO` | Grupo 3 transmitiendo por red |
| `COMPLETADO` | Respaldo exitoso |
| `FALLIDO` | Error en cualquier etapa |

---

## Pruebas

```powershell
cd control-nucleo-test
dotnet run
```

**27 pruebas** que cubren:
- VerificadorCambios (7): SHA256, comparación, excepciones
- GestorLog (5): persistencia JSON, acumulación, orden
- EjecutarRespaldoUseCase (8): flujo completo con mocks
- Entidades (7): valores por defecto, propiedades

---

## Tecnologías

| Componente | Tecnología |
|---|---|
| Lenguaje | C# |
| Framework | .NET 10.0 |
| UI | Windows Forms |
| Compresión ZIP | System.IO.Compression (nativo) |
| Compresión LZMA/RAR | 7z / rar.exe (CLI externo) |
| FTP | FluentFTP 54.2.0 |
| SFTP/SSH | SSH.NET 2025.1.0 |
| Testing | xUnit 2.9.3 + Moq |
| Contrato | OpenAPI 3.0.3 |

---

## Contrato (SDD)

Definido en `doc/contratoSDD.yaml`:

- `POST /backups/ejecutar` — Ejecuta un respaldo (Timer o botón)
- `GET /backups/historial` — Obtiene el registro de respaldos
- `POST /config/destinos` — Registra un destino de red

---

## Requerimientos funcionales

Definidos en `doc/RFS.txt`:

- **RF2** — Soporte Legacy (DBF/Fox): detectar archivos bloqueados antes de respaldar
- **RF3** — Backup de archivos locales: registrar carpetas y leer recursivamente
- **RF4** — Sincronización dinámica: detectar cambios por hash/fecha y solo copiar lo modificado
