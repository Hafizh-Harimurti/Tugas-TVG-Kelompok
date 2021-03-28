Untuk menjalankan program ini, silahkan menjalankan file executable Transform3D.exe. file executable ini akan menjalankan executable lain bernama Render3D.exe secara otomatis. executable Render3D berfungsi untuk me-render bentuk 3 dimensi. Kedua program ini berkomunikasi menggunakan metode inter-process communication berupa anonymous pipes.

adapun kegunaan tiap-tiap menu pada program transformasi 3D ini yaitu : 
[0] Load model
Berfungsi untuk men-load model yang berisi data mengenai titik-titik pada model 3 dimensi serta triangle-triangle yang membentuk permukaan model 3 dimensi tersebut. data model yang di-load berisi json yang bisa diubah secara manual. daftar file di-load dari 

[1] Load scene
Berfungsi untuk men-load data scene yang berisi informasi berupa letak kamera, warna objek 3-dimensi, serta data pencahayaan. sama seperti data model, data scene berupa json agar mudah dibaca manusia.

[2] Save model
Menyimpan model hasil transformasi menjadi suatu file teks yang berisi representasi json dari model hasil transformasi.

[3] Save scene
Menyimpan data scene saat ini berupa file teks berisi representasi json dari scene.

[4] Perform a single transformation
Melakukan satu tahap transformasi. Terdapat menu untuk memilih jenis transformasi 3 dimensi yang hendak dilakukan.

[5] Chain multiple transformations
Melakukan lebih dari satu tahapan transformasi 3 dimensi. Terdapat juga menu untuk memilih jenis transformasi yang hendak dilakukan. Transformasi dilakukan sesuai dengan urutan dipilihnya transformasi.

[6] Inspect model
Memutar model 3 dimensi terhadap sumbu x,y dan z agar dapat lebih jelas melihat model hasil transformasi.

[7] Stop inspecting model
Berhenti memutar model dari pilihan menu ke-6.

[8] Reset model
Me-reset model ke bentuk dan posisi sebelum dilakukannya transformasi.

[9] Exit
Keluar dari aplikasi.
