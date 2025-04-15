# ELO Distribution Workflow

Startet einen ELO Workflow anhand eines Templates und verteilt ihn an Benutzer, deren E-Mail-Adressen bekannt sind.

## Features

- Holt Benutzer-IDs aus E-Mail-Adressen
- Erzeugt pro Benutzer einen eigenen Workflow-Knoten
- Verbindet diese mit einem existierenden Endknoten (z. B. `node2`)
- Startet automatisch einen aktiven Workflow

## Wichtige Hinweise

- Voraussetzung: Der Platzhalterknoten heißt `EMPFAENGER_TEMPLATE`
- Zielknoten (Ende): `node2`
- Der Code nutzt ELO-Boardmittel, keine externen Tools

## Beispielaufruf

```vb
StartVerteilerWorkflowAusTemplate("12345", "Verteilung Technischer Auftrag", emailListe)
