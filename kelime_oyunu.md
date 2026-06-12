# Çok Cevaplı Kelime Oyunu - Kapsamlı Proje Tasarım ve Mimari Belgesi

## 1. Proje Özeti ve Teknoloji Yığını
* **Backend:** C# (Oyun mantığı, doğrulama, eşleştirme, ekonomi ve mesajlaşma)
* **Kalıcı Veritabanı:** PostgreSQL (Entity Framework Core)
* **In-Memory Veritabanı:** Redis
* **Frontend:** React (Kullanıcı arayüzü, profil, market, liderlik tablosu ve oyun ekranları)
* **İletişim Protokolü:** Gerçek zamanlı oyun akışı ve mesajlaşma için WebSocket / SignalR.
* **Konsept:** Kullanıcılara birden fazla doğru cevabı olan soruların sorulduğu, zamana karşı yarışılan, sosyal etkileşimli ve tur tabanlı kelime oyunu.

## 2. Temel Oyun Mekanikleri

### 2.1. Zamanlayıcı ve Oyun Akışı
* Her soru için süre **sabit 30 saniyedir**. Süre bitiminde tur sona erer.
* Oyun, turlar (round) şeklinde ilerler ve her turda **yeni bir soru** sorulur.
* Toplamda **2 tur kazanan** oyuncu, o maçı tamamen kazanmış sayılır (Best of 3 mantığı).

### 2.2. Oyun İçi Arayüz (UI) ve Cevap Akışı
* Ekranda tahmin edilecek kelimeler için "gizli kutucuklar" (placeholder) **bulunmayacaktır**.
* Oyuncunun girdiği TÜM cevaplar (doğru veya yanlış ayrımı yapılmaksızın), kendi ekran tarafında **sırasıyla ve alt alta** bir akış (feed) şeklinde listelenecektir.
* Doğru ve yanlış cevaplar sadece görsel olarak (renk veya ikonlar ile) aynı liste içinde birbirinden ayırt edilecektir.

### 2.3. Maç Sonu ve Rövanş (Rematch) Sistemi
* Maç bittiğinde sonuç ekranı gösterilir ve oyunculara birbirlerine **"Rövanş İsteği"** gönderme butonu sunulur.
* İstek kabul edilirse, oyuncular lobiye dönmeden aynı odada sıfırdan yeni bir maça başlar.

## 3. Ekonomi ve Market Sistemi

### 3.1. Oyun İçi Kazanımlar (Core Loop)
* **Popüler Cevap Bonusu:** Her sorunun tam olarak 2 adet "popüler cevabı" vardır. 30 saniye içinde bu cevaplardan birini bulan oyuncu anında **10 Altın** kazanır.
* **Maç Sonu Ödülü:** 2 turu başarıyla tamamlayıp maçı kazanan kullanıcıya maç sonunda **5 Elmas ve 10 Altın** verilir.
* **Ödüllü Reklam:** Kullanıcılara **2 saatte bir** video izleyerek ücretsiz "Elmas" kazanma hakkı sunulur (Cooldown mekanizması).

### 3.2. Oyun İçi Jokerler (Ekonomi Tüketimi)
Kullanıcıların kazandığı birimleri harcayabilmesi için oyun esnasında kullanılabilecek jokerler bulunur:
* **+5 Saniye Jokeri (10 Altın):** Oyuncunun mevcut süresine anında 5 saniye ekler.
* **İpucu Jokeri (15 Altın):** O sorunun popüler cevaplarından birinin ilk harfini ekranda gösterir.

### 3.3. Kullanıcı Profili ve Market
* **Profil Alanı:** Kullanıcı adını güncelleme/görüntüleme, anlık Altın/Elmas bakiyesini takip etme.
* **Market Alanı:** Gerçek parayla veya oyun içi tekliflerle Altın ve Elmas satın alma bölümü.

## 4. Sosyal Sistemler ve Etkileşim

### 4.1. Arkadaşlık ve Liderlik Tablosu
* **Arkadaş Yönetimi:** Oyuncular karşılaştıkları rakiplere oyun içinden/sonundan arkadaşlık isteği gönderebilir. Özel bir panelden istekler yönetilir, arkadaşlar listelenir ve istenildiği zaman **arkadaşlıktan çıkarma** işlemi yapılabilir.
* **Liderlik Tablosu (Leaderboard):** Globalde en iyi oyuncuları (en çok maç kazananlar) ve oyuncunun sadece kendi arkadaşları arasındaki sıralamasını gösteren haftalık bir liste bulunur.

### 4.2. Mesajlaşma ve Güvenlik Kuralları
* **Kısıtlı Sohbet:** Sadece aktif olarak arkadaş listesinde ekli olan kişiler oyun dışında özel mesajlaşabilir (DM).
* **Engelleme Mantığı:** Bir kişi arkadaşlıktan çıkarıldığında, tekrar eklenene kadar **kesinlikle mesaj atamaz**.
* **Anti-Spam:** Sistem genelinde soket bağlantılarına hız limiti (Rate-Limiting) uygulanmalıdır.

## 5. Gelişmiş Doğrulama (Akıllı Sistemler)

### 5.1. Hata Toleranslı Cevap Doğrulama
* 30 saniyelik süre baskısı altındaki klavye hatalarını (typo) telafi etmek için, cevap doğrulamaları C# backend tarafında **Levenshtein Distance (Fuzzy Matching)** algoritması ile yapılacaktır. 1 veya 2 harflik küçük hatalar doğru kabul edilmelidir.

## 6. UI/UX Tasarımı ve Renk Teması

Oyunun hızlı tempolu ve rekabetçi doğasını yansıtmak, aynı zamanda 30 saniyelik odaklanma süresinde göz yorgunluğunu önlemek için **"Modern ve Koyu"** bir tema tercih edilmelidir. Frontend geliştirme sırasında aşağıdaki yönergeler ve HEX kodları kullanılabilir:

### 6.1. Renk Paleti (Önerilen HEX Kodları)
* **Arka Plan:** Koyu Lacivert / Antrasit (`#0F172A` veya `#111827`)
* **Doğru Cevap Vurgusu:** Parlak Neon Yeşil (`#10B981` veya `#22C55E`)
* **Yanlış Cevap Vurgusu:** Canlı Kırmızı / Turuncu (`#EF4444` veya `#F97316`)
* **Altın (Gold):** Parlak Kehribar (`#F59E0B` veya `#EAB308`)
* **Elmas (Diamond):** Cam Göbeği / Buz Mavisi (`#06B6D4` veya `#0EA5E9`)
* **Aksiyon / Joker Butonları:** Elektrik Moru (`#8B5CF6`) veya Pembe (`#EC4899`)

### 6.2. Arayüz ve Geri Bildirim Kuralları
* **Cevap Akışı (Feed):** Cevapların listelendiği alanda, kutucukların arka planını tamamen renklendirmek yerine sadece metin rengini değiştirmek veya kelimenin yanına yeşil bir "Tik" (`✓`), kırmızı bir "Çarpı" (`✗`) ikonu koymak arayüzün şık kalmasını sağlar.
* **Süre Sayacı:** 30 saniyelik sayaç, süre azaldıkça (örneğin son 10 saniyede) sarıdan kırmızıya dönen bir renk geçişiyle (gradient) oyuncuyu görsel olarak uyarır.

## 7. Antigravity İçin Geliştirme Görevleri (Task List)

### Backend (C# - .NET / SignalR)
1. **`GameManager` & `SessionManager`:** 30sn sayaç, Best-of-3 tur kontrolü, maç sonu ödül hesaplamaları, Rövanş eşleştirmesi ve Joker kullanımlarının (süre uzatma) soket eventleri.
2. **`ValidationEngine`:** Levenshtein mesafe algoritmasını barındıran, oyuncunun string girdisini veritabanındaki geçerli cevaplarla ve 2 "popüler" cevapla karşılaştıran sınıf.
3. **`EconomyManager`:** Popüler cevap (10 Altın), Maç sonu (5 Elmas, 10 Altın) eklemeleri, 2 saatlik reklam cooldown kontrolü ve Joker satın alım bakiyelerinden düşüş (Transaction) işlemleri.
4. **`SocialManager`:** Arkadaş ekleme/silme işlemleri. Mesaj (DM) gönderimi sırasında gönderici ve alıcının **"aktif arkadaş"** olup olmadığını doğrulayan Authorization katmanı ve Rate-Limiting/Anti-Spam middleware'i.

## 9. Eşleştirme ve "Boş Lobi" Çözümleri (Cold Start)

Oyunun ilk yayınlandığı dönemde eşleştirme sürelerinin uzamasını ve oyuncu kaybını engellemek için otomatik bir Bot (Yapay Zeka Rakip) sistemi devreye girecektir:

### 9.1. İnsansı Bot Sistemi ve Davranış Algoritması
* C# tarafında, bir oyuncu eşleştirme kuyruğunda **15 saniyeden fazla** beklerse, sistem oyuncuyu otomatik olarak bir AI Bot ile eşleştirir.
* Botun oyun içi davranış algoritması şu kurallara göre eksiksiz kodlanmalıdır:
  1. **Rastgele Gecikme (İnsansı His):** Bot cevapları anında göndermez. Klavyede yazı yazıyormuş hissi vermek için her cevaptan önce 2 ile 6 saniye arasında rastgele (`Task.Delay`) bekler. Süre azaldıkça (son 10 saniye) bu bekleme süresi hızlanabilir.
  2. **Hata ve Şaşırtma Payı:** Bot her zaman kusursuz oynamaz. Bazen yanlış kelimeler gönderir veya doğru cevabı yazarken 1-2 harf hatası (typo) yapar.
  3. **Cevap Tüketimi:** Veritabanından o soruya ait `gecerliCevaplar` listesini alır, rastgele seçerek `GameManager`'a iletir ve kullandığı cevabı kendi listesinden çıkarır (aynı cevabı iki kez vermemek için).

---

### Frontend (React & Tailwind CSS)
1. **`GameBoard` Bileşeni:** Gizli kutucuklar olmadan cevapları anlık alt alta (feed) listeleyen, altta Joker butonlarını barındıran oyun arayüzü. Bölüm 6'daki renk paleti kullanılmalıdır.
2. **`GameEndScreen`:** Maç sonucunu, kazanılan ganimetleri ve "Rövanş İste" / "Arkadaş Ekle" butonlarını barındıran component.
3. **`ProfileAndStore`:** Kullanıcı adı düzenleme, bakiye gösterimi, "Video İzle Elmas Kazan" butonu (2 saat sayacı ile) ve Market arayüzü.
4. **`SocialDashboard`:** Arkadaşları listeleme, "Arkadaşlıktan Çıkar" aksiyonu, aktif arkadaşlarla SignalR üzerinden çalışan Chat penceresi ve Global/Arkadaş Liderlik Tablosu.
6. **`BotManager` Sınıfı:** 15 saniye eşleşme bulamayan oyuncular için devreye giren, `Task.Delay` ile insansı yazma süreleri simüle eden, bazen bilerek yanlış veya hatalı kelimeler göndererek soru havuzundan oyun döngüsüne (SignalR/GameManager) cevap ileten bot algoritmasının eksiksiz yazılması.