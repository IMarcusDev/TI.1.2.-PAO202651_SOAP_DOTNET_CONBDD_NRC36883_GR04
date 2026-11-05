using System.Linq;
using System.Xml.Linq;
using ec.edu.monster.config;
using ec.edu.monster.modelo.soap;
using ec.edu.monster.pruebas.fakes;

namespace ec.edu.monster.pruebas;

public class SoapTest
{
    [Fact]
    public void TheEnvelopeQualifiesTheParametersInTheServiceNamespace()
    {
        string sobre = new SoapConstructorPeticion(RutasSoap.Namespace, "loginEmpleado")
            .Parametro("usuario", "a&b")
            .Parametro("importe", 120.5m)
            .Construir();

        XElement operacion = XDocument.Parse(sobre).Descendants().First(e => e.Name.LocalName == "loginEmpleado");

        Assert.Equal(RutasSoap.Namespace, operacion.Name.NamespaceName);
        Assert.Equal("a&b", operacion.Elements().First(e => e.Name.LocalName == "usuario").Value);
        Assert.Equal("120.50", operacion.Elements().First(e => e.Name.LocalName == "importe").Value);
    }

    [Fact]
    public void TheReaderIgnoresCaseAndNamespaces()
    {
        var lector = new SoapLectorRespuesta(SoapRespuestas.LoginOk("loginEmpleado", "Monster Bank", "Empleado"));

        Assert.True(lector.EsOk());
        Assert.Equal("Monster Bank", lector.Texto("nombre"));
        Assert.Equal("Monster Bank", lector.Texto("NOMBRE"));
    }

    [Fact]
    public void AFaultBecomesASoapFaultException()
        => Assert.Throws<SoapFallaException>(() => _ = new SoapLectorRespuesta(SoapRespuestas.Falla("boom")));

    [Theory]
    [InlineData("")]
    [InlineData("<no>")]
    public void AnEmptyOrMalformedBodyIsAnInvalidResponse(string xml)
        => Assert.Throws<RespuestaInvalidaException>(() => _ = new SoapLectorRespuesta(xml));

    [Fact]
    public void RegistrosReadsEveryItemWithItsFields()
    {
        var lector = new SoapLectorRespuesta(SoapRespuestas.ConsultaOk(
            (1, "2026-10-30T09:15:00", "Depósito", "INGRESO", "500.00", ""),
            (6, "2026-10-31T16:40:00", "Transferencia", "SALIDA", "120.50", "00200002")));

        IReadOnlyList<IReadOnlyDictionary<string, string>> registros = lector.Registros("numeroMovimiento");

        Assert.Equal(2, registros.Count);
        Assert.Equal("1", registros[0]["numeroMovimiento"]);
        Assert.Equal("00200002", registros[1]["cuentaReferencia"]);
    }
}
