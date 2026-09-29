# Stakeholderwensen — MoneyBud op de telefoon, na het prototype

**Datum:** 29 september 2026
**Stakeholder:** Axel (tevens ontwikkelaar)
**Context:** ronde 2 van het mobiele prototype (het kintsugithema) is goedgekeurd. Een gesprek over
wat er nu komt: het prototype afsluiten en de echte app voor de telefoon bouwen. De eerdere rondes:
[2026-09-29-mobiel.md](2026-09-29-mobiel.md) en de prototyperondes van dezelfde dag.
**Bron:** chatgesprek, in het Engels gevoerd

> **Vertaald uit het Engels.** De wensen zijn in het Engels gegeven en hier naar het Nederlands
> vertaald, zodat `docs/stakeholder/` één taal houdt. De formulering is dus niet letterlijk die van
> Axel; de inhoud is niet aangevuld of geïnterpreteerd.
>
> Waar een punt een antwoord was op een voorstel of vraag, staat de vraag erbij en het antwoord dat
> Axel koos, onder *Verduidelijkt*.
>
> Dit is een **bronbestand**. Het wordt niet herschreven als inzichten veranderen — nieuwe of
> gewijzigde wensen komen in een nieuwe ronde of rechtstreeks in de arc42-documentatie.

---

## Het prototype is af

Ja, dit is geweldig. Ik denk dat we dan klaar zijn met het prototype.

**Verduidelijkt:**

| Vraag | Antwoord |
|---|---|
| Het idee van een derde themakeuze met kintsugi's eigen bewegingen overal (het bord dat in scherven breekt bij het wisselen van periode enzovoort): nu bouwen, of het prototype afsluiten? | **Laat het idee vallen.** Ik breng het zelf weer ter sprake als ik het echt wil. (Niet gekozen: eerst een bewegingsronde; het idee voor later bewaren) |

## De app bouwen

Wat ik nu wil: je werkt de documentatie bij. Dan maken we het gesprek leeg, en daarna maak je
zelfstandig de mobiele app, zonder verbinding met mijn telefoon. Als je iets tegenkomt waardoor
verdergaan niet mogelijk is, mag je stoppen.

**Verduidelijkt:**

| Vraag | Antwoord |
|---|---|
| De werkwijze heeft twee goedkeuringsmomenten: de scenario's en het plan. Vervallen die voor dit increment? | **Beide vervallen; ik beoordeel achteraf.** De scenario's en het plan worden zoals altijd geschreven, maar er wordt niet op goedkeuring gewacht: er wordt doorgebouwd, en scenario's, plan en app worden aan het eind samen beoordeeld. Wat zelf beslist moest worden, wordt voor Axel op een rij gezet. (Niet gekozen: bij beide momenten stoppen; alleen bij de scenario's stoppen) |

## Desktop en telefoon gelijk — behalve de thema's

Over "de desktop- en mobiele versie moeten gelijk zijn": thema's vallen daarbuiten. Die zijn voor de
desktopversie niet nodig, dus de focus ligt alleen op de telefoonapp.

**Verduidelijkt:**

| Vraag | Antwoord |
|---|---|
| Moet de telefoon het gekozen thema (en licht/donker/systeem) onthouden tussen het openen door? | **Ja, onthouden**: bewaard bij de instellingen van de app, niet in het gegevensbestand, zodat dat bestand voor desktop en telefoon hetzelfde blijft. (Niet gekozen: altijd met *Standaard* beginnen) |

## Waar de gegevens staan

**Verduidelijkt:**

| Vraag | Antwoord |
|---|---|
| Echt gebruik begint op de telefoon. De eenvoudigste plek voor de gegevens (de eigen map van de app, via USB te kopiëren) wordt gewist als de app ooit wordt verwijderd — bijvoorbeeld als de ondertekeningssleutel verandert en een update er niet overheen installeert. Hoe vangen we dat op? | **De eigen map van de app, met een vaste sleutel.** De gegevens staan in de eigen map van de app (Android/data/…/files), via USB te kopiëren. De app wordt altijd met één sleutel van Axel ondertekend, die buiten de openbare repository wordt bewaard, zodat een update er altijd overheen installeert en er nooit iets wordt verwijderd. Axels eigen kopieën via de kabel zijn het vangnet, zoals afgesproken voor de desktop. (Niet gekozen: een gedeelde map zoals Documents/MoneyBud die het verwijderen overleeft, met eenmalige toestemming voor bestandstoegang) |

## Vragen die bij het bijwerken van de documentatie opkwamen

Bij het bijwerken van de architectuurdocumentatie kwamen nog vragen op die Axel niet eerder waren
gesteld. Ze zijn hem direct voorgelegd, niet voor later bewaard.

**Verduidelijkt:**

| Vraag | Antwoord |
|---|---|
| Vanaf welke versie moeten latere versies de gegevens van de telefoon blijven lezen? | **De versie die Axel bij de beoordeling aan het eind goedkeurt.** De beoordeling mag het bestandsformaat nog veranderen; wat vóór de goedkeuring is ingevoerd, moet misschien opnieuw. Vanaf de goedgekeurde versie blijft het bewaard. (Niet gekozen: de eerste versie die geïnstalleerd wordt) |
| Wat beschermt de geldgegevens op de telefoon? Op de desktop was dat de Windows-login. | **De eigen vergrendeling van de telefoon.** MoneyBud vraagt zelf niets. (Niet gekozen: een eigen pincode of vingerafdruk in MoneyBud) |
| Android kan de map van een app automatisch naar het Google-account back-uppen, tenzij de app dat uitzet. Voor de gegevens van MoneyBud: | **Uitzetten.** De gegevens blijven alleen op de telefoon; Axels eigen kopieën via de kabel zijn de back-up. (Niet gekozen: toestaan, als automatisch vangnet) |
| Licht/donker/systeem (*Weergave*): één instelling voor alle thema's, of één per thema? | **Eén voor alle thema's**, zoals in het prototype. (Niet gekozen: één per thema) |
| Op de desktop blijft na een mislukte opslag een regel "niet opgeslagen" staan tot opslaan weer lukt. De meldingsbalk van de telefoon verdwijnt na een paar seconden. Waar komt die blijvende regel? | **Een kleine regel onder de periode** op het hoofdscherm, in de waarschuwingskleur, tot opslaan weer lukt; ook te zien als er een paneel overheen ligt, in de kop van dat paneel. (Niet gekozen: een meldingsbalk die blijft staan) |
| Android sluit apps niet zoals Windows. Voorgesteld: "bij het sluiten nog één poging tot opslaan" wordt: als MoneyBud naar de achtergrond gaat; "een tweede start zegt dat MoneyBud al open is" is niet nodig, Android draait er altijd maar één; "onleesbare gegevens: zegt het en sluit" wordt: de melding verschijnt en de app sluit als je hem wegtikt. Akkoord? | **Akkoord.** |
