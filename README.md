# Kalkulator VB.NET

Kalkulator i thjeshtë me funksione: numrat, operatorët bazë (+, -, ×, ÷),
%, √, 1/x, memoria (MC/MR/M+/M-), historia, tema e errët/e ndritshme,
dhe vlerësimi i pikëve (≥50 kalon).

## Si funksionon

- **`index.html`** — kalkulatori (HTML + CSS + JavaScript). Punon plotësisht në klient.
- **`Program.vb`** + **`KalkulatoriServer.vbproj`** — server opsional në VB.NET që ekzekutohet
  lokalisht në terminal me `dotnet run`. Serveri dëgjon në localhost dhe në rrjet (LAN).
- **`index.html`** përdor `/api/vleresoj` nëse serveri po punon; përndryshe bën fallback
  në logjikë JavaScript.

## Si ta nisësh lokalisht

### Vetëm HTML (më e thjeshta)

Dyklik mbi `index.html` — hapet në browser.

### Me serverin VB.NET

```powershell
dotnet run
```

Terminali shfaq:
- `http://localhost:5000` (kjo makinë)
- `http://<ip-yt>:5000` (rrjeti lokal)

## Hosting publik

Faqja është statike — mund të host-oset në Vercel, GitHub Pages, Netlify, etj.
Serveri VB.NET nuk ekzekutohet në këto platforma; vlerësimi i pikëve punon
me logjikën JavaScript të integruar.
