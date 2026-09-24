# Stakeholderinterview — verdieping

**Datum:** 24 september 2026
**Stakeholder:** Axel
**Context:** vervolgvragen naar aanleiding van het
[interview van 24 september](2026-09-24-interview.md)

> Vastgelegd als aanvulling, niet als vervanging. Het oorspronkelijke interview blijft ongewijzigd.

---

# Ronde 1

## Wat "feedback" moet betekenen

Met feedback bedoelde ik vooral duidelijke overzichten van hoe het ervoor staat. Denk aan een
radiaal diagram, zoals dat ook in de app van Caleb Hammer gebruikt wordt — je hebt dan meteen een
goed beeld van wat er precies waarheen gaat.

Een maandoverzicht achteraf, met hoe je het gedaan hebt in vergelijking met je budget en misschien
ook met andere maanden, is ook geen slecht idee.

## Hoe transacties de app in komen

Voor zowel inkomsten als uitgaven zou ik starten met handmatig invoeren. Automatisch is natuurlijk
handig, maar ik heb geen idee hoe dat in zijn werk zou gaan, dus dat kan later komen.

## Scope: vermogen hoort erbij, schulden niet

Het doel is een duidelijk overzicht van inkomen en uitgaven, én een duidelijk overzicht van je
vermogen.

Naast het budgetteren van maandinkomen wil ik hier ook een overzicht hebben van al mijn vermogen,
over de verschillende rekeningen heen.

Ik heb zelf geen schulden, dus dat kan voor nu buiten de scope blijven.

---

# Ronde 2

## Plek en doel zijn twee verschillende dingen

> Dit is het kernidee dat in deze ronde naar boven kwam.

We moeten onderscheid maken tussen **waar het geld zich bevindt** — betaalrekening, spaarrekening,
aandelenrekening, fysiek contant — en **waar het voor bedoeld is**, het budgetpotje.

Overblijfsels gaan dus in een budgetpotje, dat op zijn beurt op een specifieke plek staat.

## Saldo van een rekening

Allebei. Je kunt het saldo zelf vrij bijwerken, en daarnaast kunnen we zoveel automatische
berekeningen toevoegen als we tijd voor hebben.

De simpele variant kan er meteen in: de spaarrekening krijgt erbij wat ik daarvoor gebudgetteerd
heb. Moeilijkere berekeningen kunnen we later toevoegen.

## Begin van de budgetmaand

Instelbaar.

## Beleggingen

Voor nu alleen wat ik zelf invul. Mogelijk kan ik later kijken of het mogelijk is om dit
realistisch te integreren.

---

# Ronde 3

## Wat de eerste versie moet kunnen

- Inkomsten en uitgaven handmatig invoeren, met een label erop
- Potjes met bedragen: categorieën aanmaken, er een maandbedrag aan hangen, zien wat er nog over is

Rekeningen en vermogen, en terugkerende posten, horen **niet** bij de eerste versie.

## Wat het startscherm laat zien

Waar mijn geld heen gaat — het radiale diagram, de verdeling over categorieën in één oogopslag.

## Aannames die niet zijn tegengesproken

- MoneyBud is voor één gebruiker; er kijkt niemand mee
- Alle bedragen zijn in euro's

---

## Uitwerking van "plek en doel"

Dit onderscheid is groter dan het lijkt, dus het is hier uitgeschreven om te controleren of het
klopt met wat de stakeholder bedoelde.

Elk bedrag heeft **altijd twee eigenschappen tegelijk**, die onafhankelijk van elkaar variëren:

| | Betekenis | Voorbeelden |
|---|---|---|
| **Plek** | Waar het geld fysiek staat | Betaalrekening, spaarrekening, aandelenrekening, contant |
| **Doel** | Waar het voor bedoeld is | Boodschappen, hobby, uit huis gaan, ongebudgetteerd |

Twee bedragen op dezelfde rekening kunnen een verschillend doel hebben, en hetzelfde doel kan over
meerdere plekken verspreid staan.

**Waarom dit belangrijk is:** de twee pijlers van MoneyBud zijn hiermee geen twee losse
functionaliteiten, maar twee manieren om naar dezelfde gegevens te kijken.

- **Vermogen** = alles opgeteld, gegroepeerd op *plek*
- **Budget** = alles opgeteld, gegroepeerd op *doel*

Dat verklaart ook waarom het restant van een potje naar een spaarrekening kan: het geld verandert
van plek én van doel, en beide overzichten blijven daarna kloppen.

De simpele automatische berekening die hierboven genoemd wordt — de spaarrekening krijgt erbij wat
ervoor gebudgetteerd is — is precies zo'n verplaatsing.
