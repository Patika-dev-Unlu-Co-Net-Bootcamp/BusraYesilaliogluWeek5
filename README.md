# 5. hafta

- Yeni bir api üzerinde çalışmaya başlamıştım ödev belli olmadan önce. Bu yüzden ödevi bunun üzerinde yaptım.(Token,Migration,Middleware,Result Filter vs. ekli)
- Veriler sql'den çekildi.
- Filtreleme için: localhost:5000/api/members/all?card=jcb
- Listeleme için: localhost:5000/api/members/all?sort=lastname&dir=asc
- Arama için: localhost:5000/api/members/all?search=alicia
- Veri sayısını limitlemek için: localhost:5000/api/members/all?limit=35

- Örnek bir listeyi tutan in memory cache kullanıldı.
- Response cache kullanıldı.
- Redis'te yazan ve okuyan bir cache eklendi.


Restful api oluşturun
- Daha önce oluşturduğunuz apilerden birini kullanın
- tek bir endpoint ten arama, filtreleme ve sıralama işlemlerini yaptırın.
- apiye ait basit ayarları in memory cache de tutan ve kullanımını sağlayan bir yapı geliştirin
- endpointlerinizden en az birinde response cache mekanizmasını kullanın
- distributed cache olarak redis e yazan ve okuyan bir cache yönetim servisi yazın. sorgu adedi 100 ve üzeri olursa istenilen zaman aralığında cache yazsın ve okusun
