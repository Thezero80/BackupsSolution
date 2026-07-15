# Plan de Pruebas — Backups.Core (Grupo 1)

## Descripción

Pruebas automatizadas para validar las funcionalidades del **Grupo 1: Núcleo de Control y Orquestación** del sistema de respaldos.

---

## Componentes probados

| Componente | Archivo | Qué valida |
|---|---|---|
| **VerificadorCambios** | `Domain/Services/VerificadorCambios.cs` | Cálculo SHA256, comparación de hashes |
| **GestorLog** | `Domain/Services/GestorLog.cs` | Persistencia JSON, lectura/escritura de registros |
| **EjecutarRespaldoUseCase** | `Application/UseCases/EjecutarRespaldoUseCase.cs` | Orquestación: verificar→comprimir→transmitir |
| **SolicitudRespaldo** | `Domain/Entities/SolicitudRespaldo.cs` | Valores por defecto, asignación de propiedades |
| **RegistroRespaldo** | `Domain/Entities/RegistroRespaldo.cs` | Valores por defecto, estados del contrato |

---

## Pruebas (27 total)

### VerificadorCambios (7 pruebas)

| # | Prueba | Descripción |
|---|---|---|
| 1 | `CalcularHashArchivo_RetornaHash64Caracteres` | SHA256 hex = 64 chars |
| 2 | `CalcularHashArchivo_MismoArchivo_MismoHash` | Dos lecturas = mismo hash |
| 3 | `CalcularHashArchivo_ArchivoModificado_HashDiferente` | Modificar archivo = hash distinto |
| 4 | `ArchivoFueCambiado_HashVacio_RetornaTrue` | Sin hash previo = toujours changer |
| 5 | `ArchivoFueCambiado_HashesIguales_RetornaFalse` | Iguales = sin cambios |
| 6 | `ArchivoFueCambiado_HashesDiferentes_RetornaTrue` | Diferentes = con cambios |
| 7 | `CalcularHashArchivo_ArchivoNoExiste_LanzaExcepcion` | Ruta inexistente = FileNotFoundException |

### GestorLog (5 pruebas)

| # | Prueba | Descripción |
|---|---|---|
| 1 | `ObtenerHistorial_ArchivoNoExiste_RetornaVacia` | Sin archivo = lista vacía |
| 2 | `RegistrarEvento_PrimeraVez_CreaArchivo` | Primer evento = crea JSON |
| 3 | `RegistrarEvento_VariasVeces_AcumulaRegistros` | 3 eventos = 3 registros |
| 4 | `ObtenerHistorial_DeserializaCamposCorrectamente` | Campos se leen bien |
| 5 | `ObtenerHistorial_OrdenCronologico` | Orden de inserción se mantiene |

### EjecutarRespaldoUseCase (8 pruebas)

| # | Prueba | Descripción |
|---|---|---|
| 1 | `Ejecutar_ArchivoNuevo_CompriimeYTransmite` | Primera vez = comprimir + enviar |
| 2 | `Ejecutar_SinCambios_OmiteYRegistra` | Sin cambios = OMITIDO_SIN_CAMBIOS |
| 3 | `Ejecutar_ArchivoCambiado_CompriimeYTransmite` | Cambio detectado = flujo completo |
| 4 | `Ejecutar_TransmisionFalla_RegistraFallido` | Envío falla = FALLIDO |
| 5 | `Ejecutar_CompresionFalla_RegistraFallido` | Compresión falla = FALLIDO |
| 6 | `Ejecutar_ArchivoNoExiste_RegistraFallido` | Ruta mala = FALLIDO (catch) |
| 7 | `Ejecutar_Completado_GuardaHash` | COMPLETADO guarda hash |
| 8 | `Ejecutar_Fallido_NoGuardaHash` | FALLIDO no guarda hash |

### Entidades (7 pruebas)

| # | Prueba | Descripción |
|---|---|---|
| 1 | `SolicitudRespaldo_TipoDisparoDefaultEsTimer` | Default = "TIMER" |
| 2 | `SolicitudRespaldo_AlgoritmoDefaultEsZip` | Default = "ZIP" |
| 3 | `SolicitudRespaldo_LimiteVolumenDefaultEsCero` | Default = 0 |
| 4 | `SolicitudRespaldo_AsignarPropiedades_SeGuardan` | Asignar = conservar |
| 5 | `RegistroRespaldo_FechaRegistroDefaultEsAhora` | Fecha = DateTime.Now |
| 6 | `RegistroRespaldo_AsignarPropiedades_SeGuardan` | Asignar = conservar |
| 7 | `RegistroRespaldo_EstadosValidos_DelContrato` | 6 estados del contrato |

---

## Cómo ejecutar

```powershell
# Desde la raíz del proyecto
cd control-nucleo-test
dotnet run
```

### Salida esperada

```
========================================
  PRUEBAS — Backups.Core (Grupo 1)
========================================

--- VerificadorCambios ---
  [PASS] CalcularHashArchivo_RetornaHash64Caracteres
  [PASS] CalcularHashArchivo_MismoArchivo_MismoHash
  ...

========================================
  RESUMEN: 27/27 pasaron, 0 fallaron
========================================
```

### Si hay fallos

```powershell
dotnet run
# Salida: RESUMEN: 25/27 pasaron, 2 fallaron
# Exit code: 1
```

---

## Notas técnicas

- **No modifica la arquitectura existente** — solo referencia `Backups.Core`
- **Usa mocks** para `ICompressionService` e `ITransmissionService`
- **Limpia archivos temporales** después de cada prueba
- **Hace backup/restore** del `control_backups.json` real para no corromper datos
- **Exit code 1** si alguna prueba falla (CI-friendly)

---

## Estructura

```
control-nucleo-test/
├── Backups.Core.Tests.csproj    ← Proyecto mínimo que referencia Backups.Core
├── Program.cs                   ← Todas las pruebas (27 tests)
├── expected/
│   ├── hash-esperado.txt        ← Hash SHA256 conocido
│   └── estados-validos.txt      ← Estados del contrato
└── README.md                    ← Este archivo
```
