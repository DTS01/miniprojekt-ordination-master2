namespace shared.Model;

public class PN : Ordination {
	public double antalEnheder { get; set; }
    public List<Dato> dates { get; set; } = new List<Dato>();

    public PN (DateTime startDen, DateTime slutDen, double antalEnheder, Laegemiddel laegemiddel) : base(laegemiddel, startDen, slutDen) {
		this.antalEnheder = antalEnheder;
	}

    public PN() : base(null!, new DateTime(), new DateTime()) {
    }

    /// <summary>
    /// Registrerer at der er givet en dosis på dagen givesDen
    /// Returnerer true hvis givesDen er inden for ordinationens gyldighedsperiode og datoen huskes
    /// Returner false ellers og datoen givesDen ignoreres
    /// </summary>
    public bool givDosis(Dato givesDen)
    {
        // Tjekker om givesDen er inden for ordinationens gyldighedsperiode
        if (givesDen.dato >= startDen && givesDen.dato <= slutDen)
        {
            // Tilføjer datoen til listen over doseringsdatoer
            dates.Add(givesDen);
            return true;
        }
        else
        {
            // Hvis givesDen ikke er inden for gyldighedsperioden, ignoreres datoen
            return false;
        }
    }

    public override double doegnDosis()
    {
        // Beregner den gennemsnitlige døgndosis for ordinationen baseret på doseringsdatoer

        double sum = 0;

        // Tjekker om der er nogen datoer i listen
        if (dates.Count() > 0)
        {
            // Finder den tidligste og seneste dato i listen
            DateTime min = dates.First().dato;
            DateTime max = dates.First().dato;

            // Gennemgår alle datoerne for at finde den tidligste og seneste
            foreach (Dato d in dates)
            {
                if (d.dato < min)
                {
                    min = d.dato;
                }
                if (d.dato > max)
                {
                    max = d.dato;
                }
            }

            // Beregner antallet af dage mellem den tidligste og seneste dato
            int dage = (int)(max - min).TotalDays + 1;

            // Beregner den samlede dosis pr. dag ved at dividere den samlede dosis med antallet af dage
            sum = samletDosis() / dage;
        }

        // Returnerer den beregnede døgndosis
        return sum;
    }



    public override double samletDosis() {
        return dates.Count() * antalEnheder;
    }

    public int getAntalGangeGivet() {
        return dates.Count();
    }

	public override String getType() {
		return "PN";
	}
}
