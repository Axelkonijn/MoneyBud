# Stakeholderwensen — Opgebouwd aanpassen, en geld zonder doel op een rekening

**Datum:** 4 oktober 2026
**Stakeholder:** Axel (tevens ontwikkelaar)
**Context:** MoneyBud is sinds 30 september in echt gebruik, op de telefoon (increment 14). Na een
paar dagen gebruik mist Axel iets. Dit raakt de regels van 27 september over dekking en
*Opgebouwd* (arc42 §12, *Backing and Accumulated*), en wijkt op twee punten af van wat hij toen koos;
de documentatie legt dat vast, niet deze ronde.
**Bron:** chatgesprek, in het Engels gevoerd

> **Vertaald uit het Engels.** De wensen zijn in het Engels gegeven en hier naar het Nederlands
> vertaald, zodat `docs/stakeholder/` één taal houdt. De formulering is dus niet letterlijk die van
> Axel; de inhoud is niet aangevuld of geïnterpreteerd.
>
> Waar een punt een antwoord was op een voorstel of vraag, staat de vraag erbij en het antwoord dat
> Axel koos, onder *Verduidelijkt*. Waar hij een vraag niet begreep of zelf een vraag terugstelde,
> staat dat erbij, met de uitleg die hij kreeg.
>
> Dit is een **bronbestand**. Het wordt niet herschreven als inzichten veranderen — nieuwe of
> gewijzigde wensen komen in een nieuwe ronde of rechtstreeks in de arc42-documentatie.

---

## De wens

Ik heb de app een paar dagen gebruikt en merkte dat ik iets mis. Ik kan *Opgebouwd* niet aanpassen,
en ook geen *Opgebouwd* tussen categorieën verplaatsen. Op dit moment zet ik graag grote delen van
mijn inkomen in aandelen en sparen. Dat zijn ook categorieën die ik gebruik. Maar ik heb al
spaargeld en aandelen, dus "Opgebouwd" klopt niet echt met wat erin zit.

Ik stel me voor dat al het geld op de rekening onder water een categorie heeft, zichtbaar via
*Opgebouwd*. Als je geld op de rekening zet, is het, net als wanneer het als inkomen binnenkomt,
"niet toegewezen". Dan kan ik ervoor kiezen het in het *Opgebouwd* van categorieën te zetten.

Hoe klinkt dat? Waar zie je problemen? En heb je vragen?

**Zoals het voorstel aan hem is teruggegeven** (de lezing waarop de vragen hieronder steunen): elke
rekening krijgt naast het saldo een tweede bedrag, het geld dat geen categorie claimt. Een
beginsaldo, een overboeking naar de rekening of een correctie omhoog komt daar vanzelf in. Een nieuwe
handeling geeft dat geld een doel: een bedrag ervan naar het *Opgebouwd* van een categorie. Er gaat
daarbij geen geld tussen rekeningen, want het staat er al. *Opgebouwd* kan ook tussen categorieën
verplaatst worden. Geen van beide raakt het *Budget*, *Resterend* of *Niet toegewezen* van een
periode.

## Hoe het heet en hoe het werkt

**Verduidelijkt:**

| Vraag | Antwoord |
|---|---|
| Hoe heet het geld zonder doel op een rekening? | ***Vrij*** — "Saldo € 5.200,00 · Vrij € 5.000,00". (Niet gekozen: *Niet toegewezen*, zijn eigen woord, maar dezelfde naam als het periodebedrag; *Zonder doel*) |
| Hoe geef je dat geld een doel, of verplaats je *Opgebouwd* tussen categorieën? | **Een bedrag verplaatsen**, zoals toewijzen: "€ 5.000 van Vrij naar Sparen", of van Sparen naar Aandelen; een negatief bedrag verplaatst terug. (Niet gekozen: een nieuw *Opgebouwd* intypen, waarbij het verschil van of naar *Vrij* gaat) |
| *Opgebouwd* van Sparen (op Spaarrekening) naar Aandelen (op Aandelenrekening): wat gebeurt er? | **Het geld gaat mee**: het gaat ook tussen de rekeningen, zichtbaar in beide geschiedenissen, zoals bij het omzetten van een dekking. Hij doet dezelfde overboeking bij de bank. (Niet gekozen: alleen binnen één rekening; alleen het doel, geen geld) |

## Als een rekening in waarde daalt

**Verduidelijkt:**

| Vraag | Antwoord |
|---|---|
| Aandelenrekening daalt in waarde (een lagere saldocorrectie), tot onder wat de categorieën erop claimen. Wat dan? | In zijn eigen woorden: "De aandelenrekening is in mijn geval ook een spaarrekening, alles in één. Dus ik weet niet zeker wat de beste manier is. Er is geen automatisch systeem, dus ik zou het saldo met de hand aanpassen en het daarna over de verschillende categorieën verdelen, denk ik. Dus in dat geval, ja, *Vrij* wordt negatief." (Voorgesteld en zo gekozen: *Vrij* wordt negatief, met de markering en het label *Rood*, en wordt nooit vanzelf aangepast. Niet gekozen: *Opgebouwd* daalt vanzelf) |

## Geld dat op de spaar- of beleggingsrekening verdiend wordt

**Verduidelijkt:**

| Vraag | Antwoord |
|---|---|
| Boek je inkomen rechtstreeks op de spaar- of beleggingsrekening (rente, dividend), of komt alle inkomen op de Betaalrekening? | Hij begreep de vraag niet helemaal en antwoordde over wat hij doet met wat zijn spaar- en beleggingsrekening opbrengt: "Ik laat het daar staan. Dus ik zou de rekening rechtstreeks bijwerken. Ik ga het niet als inkomen invoeren en het dan aan een spaar- of budgetcategorie toewijzen. Ik voer het gewoon direct in als een wijziging op de rekening en werk dan het *Opgebouwd* van de categorieën bij. Als ik het echt wil gebruiken, zou ik het van de rekening verplaatsen met *Overboeken*, denk ik." |

## Opgebouwd gebruiken

Zijn vraag, tussendoor: "Als je een paar maanden verder bent en echt *Opgebouwd* hebt van die paar
maanden, kun je dat dan gebruiken? Hoe werkt dat nu? Kun je gewoon over budget gaan? Of laat het dan
nog steeds zien dat je te veel uitgeeft? Of is er geen nette manier om het *Opgebouwd*-saldo te
gebruiken?"

**De uitleg die hij kreeg:** er is nu geen nette manier. Een uitgave op de categorie verlaagt
*Opgebouwd* goed, maar de rij van die maand toont dan een negatief *Resterend* met de
over-budgetmarkering. Een negatieve toewijzing haalt geld alleen terug tot het *Budget* van deze
maand, want een *Budget* gaat niet onder nul; *Opgebouwd* uit eerdere maanden komt in geen enkele
periode terug. *Overboeken* verplaatst het geld maar niet het doel: *Opgebouwd* blijft staan, en het
geld komt op de Betaalrekening buiten *Niet toegewezen*, zodat het niet te budgetteren is.

**Verduidelijkt:**

| Vraag | Antwoord |
|---|---|
| Hoe wil je *Opgebouwd* eruit halen om het uit te geven? | **Verplaatsen naar *Niet toegewezen***: dezelfde verplaatshandeling, met het *Niet toegewezen* van deze periode als bestemming. Het geld gaat van de dekkende rekening naar de Betaalrekening, in plaats van zijn overboeking; daarna wijst hij het toe aan waar hij het aan uitgeeft. (Niet gekozen: direct uitgeven vanuit de categorie, zonder over-budgetmarkering zolang *Opgebouwd* het dekt; beide; nu niet) |

## De Betaalrekening

**Verduidelijkt:**

| Vraag | Antwoord |
|---|---|
| Toont de hoofdrekening (Betaalrekening) *Vrij*? | Eerst: "Ik denk dat ik het probleem niet begrijp. Waarom kan *Vrij* niet zijn zoals het *Opgebouwd* van *Niet toegewezen*?" Het voorstel daarop: de Betaalrekening krijgt geen *Vrij*, omdat *Niet toegewezen* die rol daar al heeft. Zijn antwoord: "Als je bedoelt dat *Niet toegewezen* dat al doet, en dat het *Opgebouwd* van *Niet toegewezen* de plaats van *Vrij* inneemt, dan denk ik het wel? Bedoel je dat?" |

**De uitleg die hij daarop kreeg:** *Niet toegewezen* heeft geen *Opgebouwd*. Het hoort bij één
periode, en wat aan het eind over is gaat naar de categorie bij *Restant naar*. Dus de Betaalrekening
houdt normaal alleen het geld van deze periode, en het geld daarop zonder doel is het *Niet
toegewezen* van deze periode. Daarom geen *Vrij* op de Betaalrekening. Als *Restant naar* leeg is,
blijven restanten op de Betaalrekening staan zonder dat een bedrag ze toont, zoals nu al. Hij kwam
daar niet op terug.

## Een categorie zonder rekening ("—")

De eerste vraag ging over het ontkoppelen van een categorie die zowel door MoneyBud verplaatst geld
als *Vrij* gemaakt geld bevat. Hij begreep hem niet en vroeg om een betere uitleg. Bij de tweede
versie zag hij wat hem dwarszat:

"Nu begrijp ik mijn misverstand, want beide zijn een beetje onlogisch. Als ik 5300 op mijn
spaarrekening heb en dan de categorie ontkoppel, moet het nog steeds op die rekening staan. In het
echt zou ik het nog terug moeten zetten naar de juiste rekeningen, of had ik dat al gedaan. Dus
waarom houd je het niet op de rekening waar het het laatst stond? In dit geval blijft al het geld
staan waar het is op het moment dat het op '—' gezet wordt."

Op de vraag wat er met het geld van *deze* maand gebeurt bij "—": "Kan het niet gewoon meegaan naar
wat de categorie op dat moment dekt? Dus alles van de huidige maand gaat naar waar het op dat moment
aan gekoppeld is. Als dat '—' is, blijft het staan waar het is, tot het anders wordt ingesteld. Of
maakt dat het moeilijk bij te houden?"

Over het omzetten van de dekking: "Als je bedoelt dat de dekking van Spaarrekening naar
Aandelenrekening gaat, dan neemt het het geld mee, ja. Als je eerst '—' kiest en daarna
Aandelenrekening, moet hetzelfde gebeuren."

Daarna vroeg hij waarom het geld van deze maand problemen geeft bij de sweep: "Het staat toch nog
steeds op Sparen? Waarom kan het niet gewoon bij de rest van het geld blijven?" En: "Is dat iets van
hoe de sweep werkt? Dat als de dekking '—' is, hij aanneemt dat het op de Betaalrekening staat?"

**De uitleg die hij kreeg:** ja. Het is de basisregel achter elke categorie zonder rekening, door
hem bevestigd op 27 september: haar geld staat op de Betaalrekening. Toewijzen verplaatst dan niets,
haar uitgaven worden met de Betaalrekening ingevuld, en aan het eind van de maand haalt de sweep haar
restant van de Betaalrekening. Blijft het geld van deze maand op de spaarrekening staan, dan haalt de
sweep het van de Betaalrekening, waar het nooit stond. Laat de sweep zo'n categorie overslaan, dan
blijft wat er tijdens "—" aan wordt toegewezen ongezien op de Betaalrekening staan. De sweep laten
ophalen waar het geld echt staat, betekent dat "—" niet meer zegt waar het geld is: een dekking onder
een andere naam.

**Verduidelijkt:**

| Vraag | Antwoord |
|---|---|
| Sparen: € 5.000 opgebouwd in eerdere maanden, € 150 van deze maand over, alles op Spaarrekening. *Staat op* gaat naar "—", later naar Aandelenrekening. | **Deze maand terug, de rest blijft staan.** De € 150 gaat naar de Betaalrekening, zodat "—" blijft betekenen dat het geld op de Betaalrekening staat. De € 5.000 blijft op Spaarrekening, nog steeds het *Opgebouwd* van Sparen. Wordt er weer een rekening gekozen, dan gaat de € 5.000 mee en loopt *Opgebouwd* door. Dat geldt ook bij rechtstreeks omzetten naar een andere rekening. (Niet gekozen: alles blijft staan en de sweep slaat Sparen over; alles blijft staan en de sweep werkt zoals nu) |

## Het bestandsformaat

Toen genoemd werd dat het bestandsformaat leesbaar moet blijven: "Dit wordt hoe dan ook geen
probleem. Ik heb het pas een paar dagen gebruikt en begin waarschijnlijk toch opnieuw."

**Verduidelijkt:**

| Vraag | Antwoord |
|---|---|
| De belofte voor de gegevens (ADR 0014) zegt dat elke latere MoneyBud versie 7 leest. Die belofte houden voor deze wijziging? | "Maak de keuze zelf, op basis van wat efficiënt is." **Gekozen door de documentatie: de belofte houden.** Versie 7 is gewoon gegevens waarin nog niets *Vrij* een doel kreeg; dat lezen kost een paar regels en een test, minder dan een uitzondering in ADR 0014 vastleggen, en de belofte houdt waarde vanaf de eerste keer dat ze op de proef staat. Opnieuw beginnen blijft zijn eigen keuze. |

## Vervolgvragen na het bijwerken van de documentatie

Bij het uitschrijven bleven veertien punten open. Ze zijn dezelfde dag aan Axel voorgelegd, elk met
een aanbeveling.

### Betalen vanaf een andere rekening

De eerste twee vragen gingen over een uitgave op een gedekte categorie die vanaf een andere rekening
betaald was. Hij begreep niet hoe die situatie kon ontstaan: "Waarom kunnen we uitgaven doen vanaf
een rekening die er niet aan gekoppeld is? Ik denk dat we daar eerst naar moeten kijken en het
doorpraten."

**De uitleg die hij kreeg:** het uitgaveformulier heeft een lijst *Rekening*. Bij een categorie met
een rekening staat die rekening ingevuld, maar een andere kan gekozen worden (gekozen op 27
september), bijvoorbeeld als er aan de kassa met de pas van de Betaalrekening betaald is. Dan daalt
*Opgebouwd*, maar het geld op de gekoppelde rekening niet, en lopen die twee uit elkaar. Met de nieuwe
verplaatshandeling is dat nauwelijks nog nodig: *Opgebouwd* naar *Niet toegewezen*, toewijzen, en
vanaf de Betaalrekening betalen.

**Verduidelijkt:**

| Vraag | Antwoord |
|---|---|
| Kan een uitgave op een categorie met een rekening (Sparen → Spaarrekening) vanaf een andere rekening betaald worden? | **Nee, altijd vanaf haar eigen rekening.** Een uitgave op Sparen staat altijd op Spaarrekening; de lijst ligt daarvoor vast. Categorieën zonder rekening houden de lijst (contant, een andere pas). (Niet gekozen: alles vastleggen, zonder lijst op het formulier; zoals nu laten) |

### De rest

**Verduidelijkt:**

| Vraag | Antwoord |
|---|---|
| Sparen staat op "—", met € 5.000 uit eerdere maanden op Spaarrekening. Toont de rij van Sparen dat? | **Ja**: "Opgebouwd € 5.000,00 op Spaarrekening". (Niet gekozen: nee, het telt alleen als geclaimd op Spaarrekening) |
| Sparen staat op "—". Deze maand geef je € 50 uit aan Sparen, vanaf de Betaalrekening. Daalt de € 5.000 naar € 4.950? | **Nee.** De € 50 gaat van het budget van deze maand af en telt in *Resterend*; ook de € 5.000 verlagen zou dubbel tellen. Wie van de € 5.000 wil uitgeven, verplaatst eerst naar *Niet toegewezen*. (Niet gekozen: ja) |
| Sparen op "—" met € 5.000 op Spaarrekening: kun je eruit verplaatsen, of erbij? | **Eruit wel, erbij niet.** Erbij vanuit *Vrij* kan pas weer als er een rekening is gekozen. (Niet gekozen: beide; geen van beide) |
| Het *Opgebouwd* van Beleggen is € 2.000, helemaal vanuit *Vrij*. Kan Beleggen verwijderd worden? | **Nee, niet zolang *Opgebouwd* niet nul is.** Eerst eruit verplaatsen, of archiveren. (Niet gekozen: ja, het gaat terug naar *Vrij*) |

**In één keer aangenomen, zoals aanbevolen:**

- Verplaatsen naar *Niet toegewezen* kan alleen in de huidige periode, zoals toewijzen.
- Terugdraaien van € 500 van Sparen naar *Niet toegewezen* gaat door € 500 aan Sparen toe te wijzen;
  *Niet toegewezen* is alleen een bestemming.
- *Vrij* op Spaarrekening kan ook naar een categorie op Aandelenrekening, of rechtstreeks naar *Niet
  toegewezen*. Eén regel: het geld gaat mee zodra de twee kanten op verschillende rekeningen staan.
- € 3.000 van *Vrij* naar Sparen op dezelfde rekening krijgt een alleen-lezen-regel in de
  geschiedenis van Spaarrekening.
- Meer verplaatsen dan er is (€ 500 uit € 300) gaat door; Sparen toont dan −€ 200 met *Rood*. Nooit
  geblokkeerd.
- Eén formulier *Verplaatsen* (*Van*, *Naar*, *Bedrag*): op de desktop naast *Overboeken* en vanaf de
  rij van een gedekte categorie, op de telefoon vanuit het ⋯-menu van de categorie.
- Een inkomen rechtstreeks op Spaarrekening telt dubbel (in *Niet toegewezen* en in *Vrij*): zo
  laten; hij doet dat niet.
- € 1.900 is naar Sparen geveegd, daarna gaat Sparen op "—" en blijft de € 1.900 op Spaarrekening.
  Blijkt later € 50 te veel geveegd, dan haalt *Restant bijwerken* die € 50 terug uit die € 1.900.

### Twee laatste vragen

**Verduidelijkt:**

| Vraag | Antwoord |
|---|---|
| Sparen heeft deze maand een budget van € 300 en er is al € 350 aan uitgegeven, vanaf de Betaalrekening, want Sparen had nog geen rekening. *Resterend* is −€ 50. *Staat op* gaat naar Spaarrekening (€ 5.000). Wat gebeurt er? | **€ 50 gaat de andere kant op**: van Spaarrekening naar de Betaalrekening, om de overschrijding te dekken. Sparen toont *Opgebouwd* −€ 50 (*Rood*), Spaarrekening € 4.950, *Vrij* € 5.000. Het spiegelbeeld: een overschreden categorie op "—" zetten verplaatst de overschrijding terug. (Niet gekozen: er gaat niets, zoals tot nu toe) |
| Sparen kreeg op 10 oktober Spaarrekening. In september had Sparen geen rekening, en een cadeau van € 15 voor Sparen is met de pas van de Betaalrekening betaald. Op 12 oktober wordt het bonnetje ingevoerd, gedateerd 15 september. Op welke rekening? | In zijn eigen woorden: "Ik zou deze situatie niet eens hebben, maar je hebt het met de Betaalrekening betaald, dus natuurlijk moet het van de Betaalrekening af. Maar ik zou gewoon een categorie kiezen die de Betaalrekening als rekening heeft om dit op te lossen. Ik weet niet zeker of we hier volledige logica voor nodig hebben. Maar ik denk dat we dat kunnen doen." **Genomen zoals aanbevolen**: de Betaalrekening, met de lijst open, omdat Sparen in september geen rekening had; de lijst ligt vast vanaf de maand waarin de categorie haar rekening kreeg. (Niet gekozen: altijd Spaarrekening) |

## Vragen bij het schrijven van de scenario's

**Verduidelijkt** (elk zoals aanbevolen):

| Vraag | Antwoord |
|---|---|
| Sparen op Spaarrekening toont *Opgebouwd* −€ 50. *Staat op* gaat naar Aandelenrekening. Gaat het tekort mee? | **Ja**: € 50 gaat van Aandelenrekening naar Spaarrekening, zoals een tekort meegaat bij "—". (Niet gekozen: er gaat niets) |
| Sparen, budget € 300. Op 5 oktober € 100 voor Sparen betaald vanaf de Betaalrekening (nog geen rekening). Op 10 oktober krijgt Sparen Spaarrekening, € 200 gaat erheen. Later wordt dat bonnetje van € 100 verwijderd. *Opgebouwd* wordt € 300, maar Spaarrekening houdt er € 200 voor. Wat gebeurt er? | **MoneyBud verplaatst de € 100**: het bonnetje blijft op de Betaalrekening, en € 100 gaat van de Betaalrekening naar Spaarrekening. (Niet gekozen: het bonnetje zelf verhuist; zo laten) |
| € 200 per vergissing van *Vrij* aan Vakantie gegeven en weer teruggezet. *Opgebouwd* is € 0, maar twee regels in de geschiedenis van Spaarrekening noemen Vakantie. Kan Vakantie verwijderd worden? | **Nee, archiveren.** (Niet gekozen: ja, de regels gaan mee) |
| Dat bonnetje van € 100 van 5 oktober, betaald vanaf de Betaalrekening voordat Sparen op 10 oktober Spaarrekening kreeg, wordt geopend om te wijzigen. Wat doet het veld *Rekening*? | **Het blijft de Betaalrekening, vast.** (Niet gekozen: een andere rekening kiezen kan) |

## Vragen bij het bouwen

Tijdens het bouwen, nadat het plan was goedgekeurd, kwamen nog twee punten op. Ze zijn dezelfde dag aan
Axel voorgelegd.

**Een testvoorbeeld dat botste met een eerdere regel.** In een goedgekeurd scenario kreeg een
spaarrekening een *Startsaldo* van € 0 van vandaag, en daarna een bonnetje van gisteren. Een
*Startsaldo* is wat de bank op die dag zei, en bevat dus alles van eerder; het saldo bleef € 300, waar
het scenario € 250 verwachtte. De eerste uitleg, in de termen van de test, begreep hij niet: "Ik begrijp
niet helemaal wat wat betekent. Wat bedoel je met deposit en balance en zo, ik heb het gevoel dat ik de
situatie waar we het over hebben niet echt begrijp." Na de uitleg in MoneyBud's eigen woorden:

**Verduidelijkt:**

| Vraag | Antwoord |
|---|---|
| Hoe wordt het testvoorbeeld rechtgezet? | In zijn eigen woorden: "Wat bedoel je met de rekening eerder laten beginnen? Zoals in de test zelf? Zoals: in de test hebben we de rekening eerder laten beginnen? Zo ja, dan vind ik het goed, maakt me niet echt uit. Het lijkt erop dat je wel begrijpt dat als we het saldo van een rekening op een bepaald bedrag zetten, en dan een uitgave op een eerdere datum toevoegen, dat het saldo niet verandert, omdat al duidelijk was wat het saldo op die datum was." **Gekozen: in de test begint de rekening op de eerste dag van de periode.** (Niet gekozen: € 300 verwachten; de saldoregels uit het voorbeeld halen) |
| In *Verplaatsen* kan *Vrij* op de ene rekening als *Van* en *Vrij* op een andere als *Naar* gekozen worden. Er krijgt dan niets een doel; het geld gaat alleen tussen de rekeningen, zoals bij *Overboeken*. Wat moet er gebeuren? | **Weigeren**, zoals aanbevolen: "Geld van Vrij naar Vrij verplaatsen is overboeken." (Niet gekozen: toestaan, als een tweede manier van overboeken die niet gewijzigd of verwijderd kan worden) |

## Na het installeren op de telefoon

Na het installeren van de nieuwe versie zag Axel dat de Betaalrekening geen *Vrij* heeft (regel 5 van
deze ronde), en dat er dus geen manier is om het geld dat al op de Betaalrekening stond een doel te geven.
Hem werd uitgelegd hoe dat erdoor kwam: *Niet toegewezen* bevat alleen geld dat als inkomen binnenkwam;
een startsaldo of correctie op de Betaalrekening wordt nooit *Niet toegewezen*. In zijn eigen woorden:
"Ik ben zo in de war, hoe is dit erdoor gekomen? Waarom zou ik het geld dat vrij is op de hoofdrekening
niet naar een categorie willen verplaatsen? Of mis ik iets?"

**Verduidelijkt:**

| Vraag | Antwoord |
|---|---|
| Krijgt de Betaalrekening ook *Vrij*: het *Saldo*, min het *Niet toegewezen* en het *Resterend* van categorieën zonder rekening van deze periode, min het *Opgebouwd* van categorieën die de Betaalrekening dekt, en te kiezen in *Verplaatsen*? | "Ja natuurlijk. Op dit moment heb ik geen makkelijke manier om geld van mijn Betaalrekening toe te wijzen aan een categorie op die rekening. Ik zou het naar een andere rekening moeten overboeken en het dan van daaruit op een categorie zetten. En als ik het saldo zou corrigeren, zou ik geen makkelijke manier hebben om geld uit een categorie naar *Vrij* te verplaatsen om het daarvandaan te laten afgaan als ik het saldo verlaag." **Dit herziet regel 5.** |
| De poorten voor deze aanpassing? | "Je mag de poorten overslaan." Scenario's, plan en werkende app worden samen voorgelegd, met elke beslissing die zonder hem genomen is. Het werk gaat door in een volgende sessie. |

**Bij het bouwen, 5 oktober 2026.** Deze twee vragen zijn in het Nederlands gesteld, elk als
meerkeuzevraag met een aanbeveling; hij koos beide keren de aanbeveling.

| Vraag | Antwoord |
|---|---|
| Je verlaagt het saldo van de Betaalrekening met €30 (iets vergeten). *Vrij* op de Betaalrekening wordt dan −€30. Hoe dek je dat af? Nu is *Niet toegewezen* bij *Verplaatsen* alléén een bestemming (geld eruit halen is toewijzen), maar voor *Vrij* op de Betaalrekening bestaat geen toewijzing die hetzelfde doet. Aanbevolen: ook *Niet toegewezen* → *Vrij*, alleen naar *Vrij* op de rekening die *Niet toegewezen* houdt; er beweegt geen geld, het is een regel in de geschiedenis. Afgewezen alternatief: nee, alleen uit categorieën met een rekening. | **Ook *Niet toegewezen* → *Vrij*** (de aanbeveling). Zo dekt hij een lagere correctie af met geld van deze periode, ook van categorieën zonder rekening (eerst een negatief bedrag toewijzen, dan *Niet toegewezen* → *Vrij*), en is een *Vrij* → *Niet toegewezen* per ongeluk terug te draaien. |
| Een afgelopen periode verandert achteraf (bijv. een late terugbetaling van €40 in september). Het geld staat op de Betaalrekening en de regel van september zegt "nog te verdelen €40" met *Restant bijwerken*. Zonder *Restant naar* blijft elke afgelopen periode zo vragen. Waar hoort dat geld bij? Aanbevolen: bij de regel, niet in *Vrij*. Afgewezen alternatief: in *Vrij*, en de regel blijft. | **Bij de regel, niet in *Vrij*** (de aanbeveling), en in beide richtingen: wat de regel nog vraagt (nog te verdelen, of te veel verdeeld zolang er iets terug te halen is) blijft geclaimd op de Betaalrekening tot *Restant bijwerken* het verplaatst of het wordt losgelaten. Zo staat dat geld op één plek, de regel, en niet ook in *Vrij*. Zonder *Restant naar* blijft het restant van een afgelopen periode bij zijn regel en wordt het geen *Vrij*. |

**Na het proberen, 5 oktober 2026.** Axel probeerde de versie met *Vrij* op de Betaalrekening op zijn
telefoon. In zijn woorden, vertaald: "Perfect. Een kleine fout die nog opgelost moet worden: bij het
invullen van *Verplaatsen* bedekt het toetsenbord het invoerveld. Zet het er misschien in als bug, dan
kunnen we het makkelijk in een nieuw gesprek oplossen. De rest mag gecommit en gepusht worden."
