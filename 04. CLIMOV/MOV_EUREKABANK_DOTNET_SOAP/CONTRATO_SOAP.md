# Contrato SOAP que consume el cliente móvil

El cliente móvil implementa las mismas operaciones del ERS (RNF-001) y del ECUD que el cliente Android de `TI.1.1/04. CLIMOV/MOV_EUREKABANK_JAVA_SOAP`, adaptadas a un servidor ASP.NET Core con SoapCore. Este documento fija las operaciones, los parámetros y los campos de respuesta que lee el cliente, para que el servidor los publique igual.

- URL base: `http://<IP>:5171/` (perfil `http` del servidor), que se configura en `Config/SoapConfig.cs` (`UrlBase`). En el emulador, `10.0.2.2` es el equipo anfitrión.
- Servicios: `LoginWebService.svc` y `MovimientoWebService.svc` (`UseSoapEndpoint` en `Program.cs` del servidor).
- Namespace: `http://tempuri.org/` (el que publica SoapCore para un `[ServiceContract]` sin `Namespace`). Si el servidor define otro, se cambia `SoapConfig.Namespace`.
- `SOAPAction`: `http://tempuri.org/<Contrato>/<operación>`, con contratos `ILoginWebService` e `IMovimientoWebService`. Los nombres de operación se fijan con `[OperationContract(Name = "...")]`.
- Estilo: SOAP 1.1 document/literal wrapped. El elemento de la operación declara el namespace por defecto, así los parámetros quedan calificados en él, como los envía un cliente WCF.
- Todas las respuestas llevan `estado` (`OK` o `ERROR`) y `mensaje`. El cliente lee los campos por su nombre local sin distinguir mayúsculas (`Estado` o `estado`), en cualquier orden o nivel de anidamiento.

## LoginWebService

| Operación | Parámetros | Campos de respuesta si `estado = OK` |
|---|---|---|
| `loginEmpleado` | `usuario`, `clave` | `token`, `perfil` (`EMPLEADO`), `nombre` |
| `loginCliente` | `cuenta`, `clave` | `token`, `perfil` (`CLIENTE`), `nombre` (titular) |
| `logout` | `token` | — |

## MovimientoWebService

| Operación | Parámetros | Campos de respuesta si `estado = OK` |
|---|---|---|
| `consultarMovimientos` | `token`, `cuenta` | Cuenta: `numeroCuenta`, `titular`, `moneda`, `saldo`, `estadoCuenta`. Movimientos: una lista de elementos con `numeroMovimiento`, `fecha`, `tipoMovimiento`, `accion` (`INGRESO`/`SALIDA`), `importe`, `cuentaReferencia` |
| `registrarDeposito` | `token`, `cuenta`, `importe` | `numeroMovimiento`, `nuevoSaldo` |
| `registrarRetiro` | `token`, `cuenta`, `importe`, `clave` | `numeroMovimiento`, `nuevoSaldo` |
| `registrarTransferencia` | `token`, `cuentaOrigen`, `cuentaDestino`, `importe`, `clave` | `numeroMovimiento`, `nuevoSaldo` (de la cuenta de origen) |

Notas para el servidor:

- `importe` llega como texto decimal con punto y dos decimales (`120.50`); en el servidor conviene declararlo `decimal`.
- Cuando opera el cliente, `cuenta`/`cuentaOrigen` es la cuenta de su sesión y `clave` llega vacía (ECUD3 paso 6).
- El estado de la cuenta se llama `estadoCuenta`, no `estado`, para no confundirlo con el estado de la respuesta.
- `fecha` puede enviarse como `xs:dateTime` (`2026-10-31T16:40:00`) o ya formateada (`31/10/2026 16:40`).
- La lista de movimientos puede ir envuelta en un contenedor (`<Movimientos><MovimientoDTO>…`), que es lo que genera `XmlSerializer` para un `List<MovimientoDTO>`, o como elementos repetidos.
- Si el token no es válido, el `mensaje` debe contener la palabra "Token" (ECUD4: "Token inválido o sesión expirada"). Así el cliente lo reconoce, borra la sesión y vuelve al inicio de sesión.
- Para un error de negocio, el cliente muestra el `mensaje` tal como llega. Si el mensaje contiene "clave", el cliente borra el campo de clave. Un SOAP Fault se muestra como un error genérico del servidor.

## Ejemplo de respuesta de `consultarMovimientos`

```xml
<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
  <s:Body>
    <consultarMovimientosResponse xmlns="http://tempuri.org/">
      <consultarMovimientosResult>
        <Estado>OK</Estado>
        <Mensaje>Consulta de movimientos exitosa</Mensaje>
        <Cuenta>
          <NumeroCuenta>00100001</NumeroCuenta>
          <Titular>Ana Pérez</Titular>
          <Moneda>USD</Moneda>
          <Saldo>379.50</Saldo>
          <EstadoCuenta>ACTIVO</EstadoCuenta>
        </Cuenta>
        <Movimientos>
          <MovimientoDTO>
            <NumeroMovimiento>1</NumeroMovimiento>
            <Fecha>2026-10-30T09:15:00</Fecha>
            <TipoMovimiento>Depósito</TipoMovimiento>
            <Accion>INGRESO</Accion>
            <Importe>500.00</Importe>
            <CuentaReferencia />
          </MovimientoDTO>
        </Movimientos>
      </consultarMovimientosResult>
    </consultarMovimientosResponse>
  </s:Body>
</s:Envelope>
```

Las pruebas de `pruebas/PruebasRepositorios.cs` usan estas mismas respuestas y comprueban la URL, la `SOAPAction` y el sobre que envía el cliente.
