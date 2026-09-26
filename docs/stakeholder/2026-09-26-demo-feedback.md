# Stakeholderfeedback — eerste demo

**Datum:** 26 september 2026
**Stakeholder:** Axel (tevens ontwikkelaar)
**Context:** een begeleide demosessie met de eerste versie met scherm (increment 5, de desktop-UI).
Axel doorliep een korte takenlijst in de app en gaf gaandeweg zijn reacties.
**Bron:** chatgesprek, in het Engels gevoerd

> **Vertaald uit het Engels.** De feedback is in het Engels gegeven en hier naar het Nederlands
> vertaald, zodat `docs/stakeholder/` één taal houdt. De formulering is dus niet letterlijk die van
> Axel; de inhoud is niet aangevuld of geïnterpreteerd. Opgeschoond en per onderwerp gegroepeerd.
>
> Waar een punt pas na een vervolgvraag duidelijk werd, staat de vraag erbij en het antwoord dat
> Axel koos, onder *Verduidelijkt*.
>
> Dit is een **bronbestand**. Het wordt niet herschreven als inzichten veranderen — nieuwe of
> gewijzigde wensen komen in een nieuwe ronde of rechtstreeks in de arc42-documentatie.

---

## Het categorievak maakt zichzelf niet leeg

Als ik iets in het categorievak invul, wordt het na gebruik niet leeggemaakt. Voorbeeld: ik typ
"groc", hij stelt "Groceries" voor, ik kies die en voeg de uitgave toe — en dan springt het vak
terug naar "groc", in plaats van leeg te worden.

## De ring is te klein en doet niets

Waar ik tegenaan loop is de opmaak. Ik had het radiale diagram veel groter voorgesteld, en
interactief. Als ik nu wat kleinere budgetten toevoeg, kan ik ze bijna niet zien in de ring, en ik
kan al helemaal niet zien of er iets van uitgegeven is.

**Verduidelijkt:**

| Vraag | Antwoord |
|---|---|
| Hoe groot moet de ring zijn? | **Het middelpunt**: de ring vult het grootste deel van de middelste kolom, met de categorierijen eronder |
| Wat moet interactie met de ring doen? | **Aanwijzen toont details**: bij een segment verschijnen categorie, Budget, Uitgegeven en Resterend. (Niet gekozen: klikken selecteert de categorie, klikken zoomt in) |
| Hoe worden kleine budgetten zichtbaar? | **Een minimale segmentbreedte**: elk segment is minstens zichtbaar breed, ook als de ring daardoor niet meer exact in verhouding is. (Niet gekozen: exact houden, of namen naast de ring) |

## De volgorde van de invoervelden

Ik merk ook dat ik de volgorde van de tekstvakken niet prettig vind. Ik vul liever eerst in *wat*
het is en daarna het bedrag, want zo denk ik over dat soort dingen.

**Verduidelijkt:**

| Vraag | Antwoord |
|---|---|
| Wat is bij een uitgave het "wat"? | **Omschrijving, dan categorie, dan bedrag, dan datum.** Bij een inkomst: omschrijving, bedrag, datum. Bij toewijzen: categorie, dan bedrag |

## Stappen tussen maanden

Voor de demo was het vervelend dat als ik naar oktober stapte en daar een inkomst wilde invullen,
de datum standaard nog in september stond. Maar in een echte situatie zou ik de datum toch moeten
aanpassen, dus het maakt minder uit.

## Wat goed ging

Het salaris invoeren was geen probleem.

## Wat nog ontbreekt, maar later komt

Natuurlijk missen we nog het verwijderen van gegevens, en het bewaren van de gegevens. Maar dat
komt later.
