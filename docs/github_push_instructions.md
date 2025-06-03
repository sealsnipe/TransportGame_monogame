# Anleitung: GitHub-Ordner in ein Repository pushen

Diese Anleitung beschreibt, wie Sie nur den Inhalt eines "github"-Ordners in ein neues GitHub-Repository namens "TransportGame_monogame" pushen können.

## Voraussetzungen

- Git ist auf Ihrem System installiert
- Sie haben ein GitHub-Konto
- Sie haben bereits ein leeres Repository namens "TransportGame_monogame" auf GitHub erstellt

## Schritt-für-Schritt Anleitung

1. **Navigieren Sie zum github-Ordner**

```bash
cd pfad/zum/github-ordner
```

2. **Initialisieren Sie ein Git-Repository**

```bash
git init
```

3. **Fügen Sie die Remote-URL hinzu**

```bash
git remote add origin https://github.com/IHR-USERNAME/TransportGame_monogame.git
```

4. **Fügen Sie alle Dateien zum Staging-Bereich hinzu**

```bash
git add .
```

5. **Erstellen Sie einen Commit**

```bash
git commit -m "Initial commit of TransportGame MonoGame"
```

6. **Pushen Sie zum main-Branch**

```bash
git push -u origin main
```

## Mögliche Probleme und Lösungen

### Problem: "fatal: remote origin already exists"
Lösung:
```bash
git remote remove origin
git remote add origin https://github.com/IHR-USERNAME/TransportGame_monogame.git
```

### Problem: Authentifizierungsfehler
Lösung:
- Überprüfen Sie Ihren GitHub-Benutzernamen und Passwort
- Bei aktivierter Zwei-Faktor-Authentifizierung müssen Sie ein Personal Access Token verwenden

### Problem: "error: failed to push some refs"
Lösung:
```bash
git pull --rebase origin main
git push origin main
```

## Überprüfung

Nach dem erfolgreichen Push sollten Sie Ihre Dateien auf GitHub unter `https://github.com/IHR-USERNAME/TransportGame_monogame` sehen können.