import sqlite3

connection = sqlite3.connect("../database/lab1.sqlite3")
cursor = connection.cursor()

tables = [
    'main1',
    'main2',
    'main2der',
    'test',
    'pars'
]

for table in tables:
    cursor.execute("delete from " + table + ";")

connection.commit()