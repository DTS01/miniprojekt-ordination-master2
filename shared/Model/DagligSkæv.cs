namespace shared.Model;

public class DagligSkæv : Ordination {
    public List<Dosis> doser { get; set; } = new List<Dosis>();

    public DagligSkæv(DateTime startDen, DateTime slutDen, Laegemiddel laegemiddel) : base(laegemiddel, startDen, slutDen) {
	}

    public DagligSkæv(DateTime startDen, DateTime slutDen, Laegemiddel laegemiddel, Dosis[] doser) : base(laegemiddel, startDen, slutDen) {
        this.doser = doser.ToList();
    }    

    public DagligSkæv() : base(null!, new DateTime(), new DateTime()) {
    }

	public void opretDosis(DateTime tid, double antal) {
        doser.Add(new Dosis(tid, antal));
    }

	public override double samletDosis() {
		return base.antalDage() * doegnDosis();
	}

    public override double doegnDosis()
    {
        double totalDosis = 0.0;

        // Beregn den samlede dosis ved at summere dosis på hvert tidspunkt
        foreach (var dosis in doser)
        {
            totalDosis += dosis.antal;
        }

        // Antager, der gives dosis hver dag i perioden, så den gennemsnitlige dosis per dag er den samlede dosis divideret med antallet af dage
        int antalTidspunkter = doser.Count;
        if (antalTidspunkter == 0)
        {
            return 0.0; // Undgå division med nul
        }

        // Antal dage kan også findes baseret på start- og slutdato i ordinationen
        int antalDage = base.antalDage();

        // Beregner den gennemsnitlige dosis per dag
        double gennemsnitligDosisPrDag = totalDosis / antalDage;

        return gennemsnitligDosisPrDag;
    }


    public override String getType() {
		return "DagligSkæv";
	}
}
