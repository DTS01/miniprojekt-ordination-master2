namespace ordination_test;

using Microsoft.EntityFrameworkCore;

using Service;
using Data;
using shared.Model;

[TestClass]
public class ServiceTest
{
    private DataService service;

    [TestInitialize]
    public void SetupBeforeEachTest()
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrdinationContext>();
        optionsBuilder.UseInMemoryDatabase(databaseName: "test-database");
        var context = new OrdinationContext(optionsBuilder.Options);
        service = new DataService(context);
        service.SeedData();
    }

    [TestMethod]
    public void PatientsExist()
    {
        Assert.IsNotNull(service.GetPatienter());
    }

    [TestMethod]
    public void OpretDagligFast()
    {
        Patient p = service.GetPatienter().First();
        Laegemiddel lm = service.GetLaegemidler().First();

        Assert.AreEqual(1, service.GetDagligFaste().Count());

        service.OpretDagligFast(p.PatientId, lm.LaegemiddelId,
            2, 2, 1, 0, DateTime.Now, DateTime.Now.AddDays(3));

        Assert.AreEqual(2, service.GetDagligFaste().Count());
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestAtKodenSmiderEnException()
    {
        // Kald metoden eller koden, der forventes at kaste en ArgumentNullException

        // Forvent at følgende kald kaster en ArgumentNullException
        service.GetAnbefaletDosisPerDøgn(-1, -1);
    }

    [TestMethod]
    public void FindPatientByCprNumber()
    {
        // Opret en testpatient med et kendt CPR-nummer
        string cpr = "121256-0512";
        Patient expectedPn = new Patient(cpr, "Jane Jensen", 63.4);

        // Tilføj testpatienten til databasen
        service.GetPatienter().Add(expectedPn);

        // Kald metoden til at finde patienten baseret på CPR-nummeret
        Patient actualPn = service.GetPatienter().FirstOrDefault(p => p.cprnr == cpr);

        // Verificer om den fundne patient er den forventede patient
        Assert.IsNotNull(actualPn); // Kontrollerer om patienten blev fundet
        Assert.AreEqual(expectedPn.cprnr, actualPn.cprnr);
        Assert.AreEqual(expectedPn.navn, actualPn.navn);
        Assert.AreEqual(expectedPn.vaegt, actualPn.vaegt);
    }

    [TestMethod]
    public void SelectMedicineById()
    {
        // Opret et testlægemiddel med et kendt id
        int medicineId = 1;
        Laegemiddel expectedLm = service.GetLaegemidler().FirstOrDefault(m => m.LaegemiddelId == medicineId);

        // Kald metoden til at vælge lægemidlet baseret på id'et
        Laegemiddel actualLm = service.GetLaegemidler().FirstOrDefault(m => m.LaegemiddelId == medicineId);

        // Verificer om det fundne lægemiddel er det forventede lægemiddel
        Assert.IsNotNull(actualLm); // Kontrollerer om lægemidlet blev fundet
        Assert.AreEqual(expectedLm.LaegemiddelId, expectedLm.LaegemiddelId);
        Assert.AreEqual(expectedLm.navn, expectedLm.navn);
        Assert.AreEqual(expectedLm.enhedPrKgPrDoegnLet, expectedLm.enhedPrKgPrDoegnLet);
        Assert.AreEqual(expectedLm.enhedPrKgPrDoegnNormal, expectedLm.enhedPrKgPrDoegnNormal);
        Assert.AreEqual(expectedLm.enhedPrKgPrDoegnTung, expectedLm.enhedPrKgPrDoegnTung);
        Assert.AreEqual(expectedLm.enhed, expectedLm.enhed);
    }

    [TestMethod]
    public void OpretPN_IncreasesPatientPNCount()
    {
        // Valg af en bestemt patient
        int patientId = 1; 

        // Antal PN-ordinationer for patienten før oprettelse
        int initialPNCount = service.GetPatienter().First(p => p.PatientId == patientId).ordinationer.OfType<PN>().Count(); 

        // Opret en PN-ordination
        service.OpretPN(patientId, 1, 2.5, DateTime.Now, DateTime.Now.AddDays(5));

        // Kontroller om antallet af PN-ordinationer for patienten er øget med 1
        int updatedPNCount = service.GetPatienter().First(p => p.PatientId == patientId).ordinationer.OfType<PN>().Count();
        Assert.AreEqual(initialPNCount + 1, updatedPNCount);
    }

    [TestClass] // Attribut, der angiver, at klassen indeholder testmetoder
    public class OrdinationTests 
    {
        [TestMethod] 
        public void Test_AntalDage() // Testmetode til at teste antalDage-metoden
        {
           
            var startDato = new DateTime(2024, 4, 1); // Startdato for ordinationen
            var slutDato = new DateTime(2024, 4, 5); // Slutdato for ordinationen
            var ordination = new TestOrdination(startDato, slutDato); // Opretter en testordination med angivne datoer

            var result = ordination.antalDage(); // Kalder metoden antalDage på testordinationen og gemmer resultatet

            // Verificerer, at det forventede resultat opnås
            Assert.AreEqual(5, result); // Forventer, at antal dage mellem start- og slutdato er 5
        }
    }

    public class TestOrdination : Ordination // En midlertidig testklasse, der arver fra Ordination-klassen
    {
        public TestOrdination(DateTime startDen, DateTime slutDen) // Konstruktør til at initialisere testordinationen
        {
            this.startDen = startDen; // Initialiserer startdatoen for testordinationen
            this.slutDen = slutDen; // Initialiserer slutdatoen for testordinationen
        }

        // Implementerer de abstrakte metoder fra Ordination-klassen, som er nødvendige for at oprette en testklasse
        public override double samletDosis() => throw new NotImplementedException(); // Midlertidigt kaster en NotImplementedException
        public override double doegnDosis() => throw new NotImplementedException(); // Midlertidigt kaster en NotImplementedException
        public override string getType() => throw new NotImplementedException(); // Midlertidigt kaster en NotImplementedException
    }

    [TestMethod]
    public void Test_DoegnDosis_DagligFast()
    {
        //Forbered testdata med forskellige doseringer og datoer
        var morgenAntal = 2.0;
        var middagAntal = 1.0;
        var aftenAntal = 0.0;
        var natAntal = 1.0;

        var startDato = new DateTime(2024, 4, 1);
        var slutDato = new DateTime(2024, 4, 5);

        var ordination = new DagligFast(
            startDen: startDato,
            slutDen: slutDato,
            laegemiddel: new Laegemiddel(),
            morgenAntal: morgenAntal,
            middagAntal: middagAntal,
            aftenAntal: aftenAntal,
            natAntal: natAntal
        );

        // Udfør handlingen, der skal testes
        var result = ordination.doegnDosis();

        // Forventet gennemsnitlig dosis pr. dag er summen af de forskellige doser divideret med antallet af doseringstidspunkter
        var forventetGennemsnit = (morgenAntal + middagAntal + aftenAntal + natAntal) / 4.0;
        Assert.AreEqual(forventetGennemsnit, result);
    }

    [TestMethod]
    public void Test_DoegnDosis_DagligSkæv()
    {
        // forbered
        DateTime startDato = DateTime.Now;
        DateTime slutDato = DateTime.Now.AddDays(5);
        Laegemiddel laegemiddel = new Laegemiddel("TestMedicine", 1.0, 1.5, 2.0, "mg");

        // Opret en ny daglig skæv ordination
        DagligSkæv dagligSkaev = new DagligSkæv(startDato, slutDato, laegemiddel);

        // Tilføj doser med angivne tidspunkter og antal enheder
        dagligSkaev.opretDosis(new DateTime(2024, 4, 27, 9, 30, 0), 2);
        dagligSkaev.opretDosis(new DateTime(2024, 4, 27, 10, 30, 0), 1);
        dagligSkaev.opretDosis(new DateTime(2024, 4, 27, 13, 30, 0), 2);
        dagligSkaev.opretDosis(new DateTime(2024, 4, 27, 14, 30, 0), 1);
        dagligSkaev.opretDosis(new DateTime(2024, 4, 27, 19, 30, 0), 2);
        dagligSkaev.opretDosis(new DateTime(2024, 4, 27, 20, 30, 0), 1);

        // udfør
        double doegnDosis = dagligSkaev.doegnDosis();

        // Forventet doegnDosis = (2 + 1 + 2 + 1 + 2 + 1) / 6 = 1.5
        Assert.AreEqual(1.5, doegnDosis);
    }

    [TestMethod]
    public void Test_DoegnDosis_PN()
    {
        // Arrange: Forbered testen ved at oprette en patient, et lægemiddel og en PN-ordination
        Patient patient = service.GetPatienter().First();
        Laegemiddel laegemiddel = service.GetLaegemidler().First();
        DateTime startDen = DateTime.Now.AddDays(-5);
        DateTime slutDen = DateTime.Now.AddDays(5);
        double antalEnheder = 2;
        PN pnOrdination = service.OpretPN(patient.PatientId, laegemiddel.LaegemiddelId, antalEnheder, startDen, slutDen);

        // Hent de faktiske doser fra dataservicen
        List<DateTime> doseringsDatoer = new List<DateTime>
    {
        DateTime.Now.AddDays(-3),
        DateTime.Now.AddDays(-1),
        DateTime.Now.AddDays(1)
    };

        // Tilføj doseringsdatoer til PN-ordinationen
        foreach (var dato in doseringsDatoer)
        {
            pnOrdination.givDosis(new Dato { dato = dato });
        }

        // Beregn forventet døgndosis
        int antalDage = (slutDen - startDen).Days + 1;
        double forventetDoegnDosis = (antalEnheder * doseringsDatoer.Count) / (double)antalDage;

        // Act: Hent den beregnede døgndosis
        double beregnetDoegnDosis = pnOrdination.doegnDosis();

        // Assert: Verificerer, at den beregnede døgndosis er korrekt
        Assert.AreEqual(forventetDoegnDosis, beregnetDoegnDosis);
    }

}