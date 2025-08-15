// MongoDB IsActive alanı temizleme scripti
// Kullanım:
// 1. mongosh ile veritabanına bağlanın.
// 2. load('scripts/mongo_cleanup_isactive.js')
// 3. cleanupIsActive('HospitalManagementDB');

function cleanupIsActive(dbName) {
  const targetCollections = ['Users', 'Departments', 'Medicines', 'Medications'];
  const conn = db.getSiblingDB(dbName);
  print('\n[Cleanup] IsActive alanı taraması başlıyor -> DB: ' + dbName);

  targetCollections.forEach(colName => {
    if (!conn.getCollectionNames().includes(colName)) {
      print('- Koleksiyon yok: ' + colName);
      return;
    }

    const col = conn.getCollection(colName);

    const countWith = col.countDocuments({ IsActive: { $exists: true } });
    if (countWith === 0) {
      print('- ' + colName + ': IsActive alanı içeren belge yok.');
      return;
    }

    print('- ' + colName + ': ' + countWith + ' belge IsActive alanına sahip. Kaldırılıyor...');
    const res = col.updateMany(
      { IsActive: { $exists: true } },
      { $unset: { IsActive: '' } }
    );
    print('  -> Güncellenen: ' + res.modifiedCount);
  });

  print('\n[Cleanup] Tamamlandı.');
}

// Otomatik çalıştırmak istersen aşağıdakini aç:
// cleanupIsActive('HospitalManagementDB');
