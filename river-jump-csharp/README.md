# River Jump C# Version

Bu proje Doodle Jump benzeri bir C# Windows Forms oyunudur. Harici oyun motoru veya paket kullanmaz.

## Calistirma

1. Visual Studio ile `RiverJump.csproj` dosyasini acin.
2. Run / Start butonuna basin.

Alternatif olarak terminalden:

```bash
dotnet run
```

## Kontroller

- Sol: `A` veya sol ok
- Sag: `D` veya sag ok
- Baslat / tekrar baslat: mouse ile tikla, `Space` veya `Enter`

## Oyun Mantigi

- Kurbaga platformlara basarak yukari cikar.
- Kutuk normal ziplatir.
- Mantar daha yuksege ziplatir.
- Ruzgar platformu oyuncuyu yukari iter.
- Dalga dokununca oyunu bitirir.
- Timsaha ustten basarsan kirilir, yandan veya alttan temas edersen oyun biter.
- En ustteki `END` platformuna ulasinca kazanirsin.

## Dosyalar

- `Program.cs`: Uygulamayi baslatir.
- `GameForm.cs`: Oyun dongusu, fizik, cizimler, skor ve carpismalar burada.
- `RiverJump.csproj`: C# proje ayarlari.
