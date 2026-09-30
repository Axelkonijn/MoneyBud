# Stakeholderwensen — de telefoonapp, beoordeling ronde 1

**Datum:** 30 september 2026
**Stakeholder:** Axel (tevens ontwikkelaar)
**Context:** increment 14, de telefoonapp, is gebouwd zonder goedkeuringsmomenten en op Axels telefoon
geïnstalleerd. Hij gebruikte hem een paar uur. Dit is zijn eerste reactie in de beoordeling die de
goedkeuringsmomenten vervangt ([na het prototype](2026-09-29-mobiel-na-het-prototype.md)).
**Bron:** chatgesprek, in het Engels gevoerd

> **Vertaald uit het Engels.** De wensen zijn in het Engels gegeven en hier naar het Nederlands
> vertaald, zodat `docs/stakeholder/` één taal houdt. De formulering is dus niet letterlijk die van
> Axel; de inhoud is niet aangevuld of geïnterpreteerd.
>
> Dit is een **bronbestand**. Het wordt niet herschreven als inzichten veranderen — nieuwe of
> gewijzigde wensen komen in een nieuwe ronde of rechtstreeks in de arc42-documentatie.

---

Ik heb hem een paar uur gebruikt en heb een paar heel kleine puntjes. De rest lijkt goed. Beide
puntjes gaan over de gebruikersinterface.

## 1. Het getal in het midden flikkert

Telkens als ik een paneel eruit of erin trek, flikkert het getal in het midden. Al het andere blijft
mooi, maar alleen het midden krimpt een beetje en flikkert. Dat leidt behoorlijk af.

## 2. Het budget hoeft niet halverwege te stoppen

Het was een functie, maar ik merk dat ik hem niet fijn vind. Je kunt het budgetvenster maar halverwege
omhoogtrekken. Dat is gewoon niet echt nodig. Het idee was dat je de ring tegelijk kon zien, maar zo
zou ik het nooit gebruiken. Dus ik wil niet alleen één enkele trek, net als bij de andere drie
kanten, maar ook dat het krimpen van de ring verdwijnt, want dat is niet nodig nu het halverwege
trekken er niet meer is.

## De beoordeling is afgerond

Geweldig. Ik denk dat we dan kunnen zeggen dat hij uit de beoordelingsfase is.

**Verduidelijkt:**

| Vraag | Antwoord |
|---|---|
| De telefoon toont drie dingen die de desktop niet toont: de totalen van inkomsten en uitgaven boven hun lijsten, "van € … inkomen" onder de ring, en de uitgaven van een categorie op haar eigen pagina. "De builds blijven gelijk", dus één kant met meer telt als een fout, tenzij het een uitzondering wordt. Wat moet er gebeuren? | **Ze op een later moment ook aan de desktop toevoegen.** (Niet gekozen: ze als uitzondering alleen op de telefoon houden; ze van de telefoon weghalen) |
| Het increment staat op de branch increment-14-phone en er is nog niets vastgelegd. Hoe ver moet het? | **Vastleggen en samenvoegen in main, en alles naar de remote pushen, ook de branches die alleen lokaal staan.** (Niet gekozen: alleen vastleggen en samenvoegen; alleen vastleggen op de branch) |