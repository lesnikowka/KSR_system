import numpy as np
import math
import sys
import sqlite3
import os

# массивы для таблицы задание 1 и тестовое
xi = []
vi = []
v2i = []
cntrl = []
olp = []
hi = []
C1 = 0
C2 = 0
C1i = []
C2i = []
ui = []
# + массивы для 2-го задания
u1 = []
u2 = []
v22i = []
cntrl2 = []
olp2 = []

g = 9.8
L = 0.1
m = 1

Nmax = 100  # количество итераций
eps = 0.001  # погрешность
e = 0.0001  # погрешность выхода на границу
p = 4  # порядок метода
hmin = 10 ** -9  # минимальный шаг(для метода с контролем погрешности)
vMax = 10  # оценка для значения v( для адекватного предстваления функции решения)
b = 2 #граница
x0 = 0 
v0 = 3
v0der = 0 
h = 0.01
WC = True #флаг для включения\отключения контроля ЛП
taskType = "main2" # выбранная задача
maximum_derivative = 10 ** 8 # максимальная производная


def catchParamsFromCmd():
    global x0, v0, h, Nmax, eps, e, WC, g, L, m, taskType, b, v0der

    if len(sys.argv) == 1:
        print("программа запущена со стандартными параметрами")
        return

    x0 = float(sys.argv[1])
    v0 = float(sys.argv[2])
    h = float(sys.argv[3])
    Nmax = int(sys.argv[4])
    eps = float(sys.argv[5])
    e = float(sys.argv[6])
    WC = bool(int(sys.argv[7]))
    g = float(sys.argv[8])
    L = float(sys.argv[9])
    m = float(sys.argv[10])
    taskType = sys.argv[11]
    b = float(sys.argv[12])
    v0der = float(sys.argv[13])


def eraseEndValues():
    if (len(xi)):
        xi[len(xi) - 1] = 0
    if (len(vi)):
        vi[len(vi) - 1] = 0
    if (len(v2i)):
        v2i[len(v2i) - 1] = 0
    if (len(cntrl)):
        cntrl[len(cntrl) - 1] = 0
    if (len(olp)):
        olp[len(olp) - 1] = 0
    if (len(hi)):
        hi[len(hi) - 1] = 0
    if (len(C1i)):
        C1i[len(C1i) - 1] = 0
    if (len(C2i)):
        C2i[len(C2i) - 1] = 0
    if (len(ui)):
        ui[len(ui) - 1] = 0
    if (len(u1)):
        u1[len(u1) - 1] = 0
    if (len(u2)):
        u2[len(u2) - 1] = 0
    if (len(v22i)):
        v22i[len(v22i) - 1] = 0


def saveCurrentValuesSystem(S, S2, xn, vn1, vn2, hn, v21n, v22n, cntrln1, cntrln2, c1, c2):
    olp.append(abs(S))
    olp2.append(abs(S2))
    xi.append(xn)
    u1.append(vn1)
    u2.append(vn2)
    hi.append(hn)
    v2i.append(v21n)
    v22i.append(v22n)
    cntrl.append(abs(cntrln1))
    cntrl2.append(abs(cntrln2))
    C1i.append(c1)
    C2i.append(c2)


def saveToDatabase():
    connection = sqlite3.connect("../database/lab1.sqlite3")
    cursor = connection.cursor()
    
    
    
    cursor.execute("delete from main2 where x0=? and u0=? and u0der=?", [x0, v0, v0der])
    cursor.execute("delete from main2der where x0=? and u0=? and u0der=?", [x0, v0, v0der])
    for i in range(len(xi)):
        if WC:
            cursor.executemany("insert into main2 values(?,?,?,?,?,?,?,?,?,?,?,?,?)",
                                  [[u2[i], x0, v0, i, xi[i], u1[i], v2i[i], cntrl[i], olp[i],
                                    hi[i], C2i[i], C1i[i], v0der]])

            cursor.executemany("insert into main2der values(?,?,?,?,?,?,?,?,?,?,?,?,?)",
                                   [[u2[i], x0, v0, i, xi[i], u2[i], v22i[i], cntrl2[i], olp2[i],
                                     hi[i], C2i[i], C1i[i], v0der]])
        else:
            cursor.executemany("insert into main2 values(?,?,?,?,?,?,?,?,?,?,?,?,?)",
                                   [[u2[i], x0, v0, i, xi[i], u1[i], v2i[i], cntrl[i], olp[i],
                                     h, 0, 0, v0der]])
            cursor.executemany("insert into main2der values(?,?,?,?,?,?,?,?,?,?,?,?,?)",
                                   [[u2[i], x0, v0, i, xi[i], u2[i], v22i[i], cntrl2[i], olp2[i],
                                     h, 0, 0, v0der]])

    connection.commit()

    

def startCalculation():    
        if WC:
            RK4WCSys(x0, v0, v0der, h, Nmax, b, e, f1sys, f2sys, eps)
        else:
            RK4Sys(x0, v0, v0der, h, Nmax, b, e, f1sys, f2sys)



def f1sys(x, v1, v2):
    return v2


def f2sys(x, v1, v2):
    return -g/L * v1 # было без минуса


# Задача №2
def func1(u1=0, u2=0):
    return u2


def func2(u1=0, u2=0, x=0):
    a, b, c = initParams()
    return -a/b * u1


# Для задачи параметров
def initParams():
    return g, L, m


def stepForSystem(x, v1, v2, h, f1, f2, withControl=False):
    k11 = f1(x, v1, v2)
    k12 = f2(x, v1, v2)
    k21 = f1(x + h / 2, v1 + h / 2 * k11, v2 + h / 2 * k12)
    k22 = f2(x + h / 2, v1 + h / 2 * k11, v2 + h / 2 * k12)
    k31 = f1(x + h / 2, v1 + h / 2 * k21, v2 + h / 2 * k22)
    k32 = f2(x + h / 2, v1 + h / 2 * k21, v2 + h / 2 * k22)
    k41 = f1(x + h, v1 + h * k31, v2 + h * k32)
    k42 = f2(x + h, v1 + h * k31, v2 + h * k32)
    xn = x + h
    vn1 = v1 + h / 6 * (k11 + 2 * k21 + 2 * k31 + k41)
    vn2 = v2 + h / 6 * (k12 + 2 * k22 + 2 * k32 + k42)
    if not withControl:
        xi.append(xn)
        u1.append(vn1)
        u2.append(vn2)
        
        xn, vn1, vn2 = stepForSystem(x, v1, v2, h, f1, f2, True)
        xHalf, v1Half, v2Half = stepForSystem(x, v1, v2, h / 2, f1, f2, True)
        xNext, v1Next, v2Next = stepForSystem(x, v1Half, v2Half, h / 2, f1, f2, True)

        S1 = (v1Next - vn1) / (2 ** p - 1)
        S2 = (v2Next - vn2) / (2 ** p - 1)

        olp.append(abs(S1))
        olp2.append(abs(S2))
        cntrl.append(abs(vn1-v1Next))
        cntrl2.append(abs(vn2-v2Next))
        v2i.append(v1Next)
        v22i.append(v2Next)

    return xn, vn1, vn2




def stepForSystemWithControl(x, v1, v2, h, f1, f2, eps):
    global C1, C2
    xn, vn1, vn2 = stepForSystem(x, v1, v2, h, f1, f2, True)
    xHalf, v1Half, v2Half = stepForSystem(x, v1, v2, h / 2, f1, f2, True)
    xNext, v1Next, v2Next = stepForSystem(x, v1Half, v2Half, h / 2, f1, f2, True)

    S1 = (v1Next - vn1) / (2 ** p - 1)
    S2 = (v2Next - vn2) / (2 ** p - 1)

    S = max(abs(S1), abs(S2))

    oldC1 = C1
    oldC2 = C2

    if abs(S) >= eps / 2 ** (p + 1) and abs(S) <= eps:
        saveCurrentValuesSystem(S1, S2, xn, vn1, vn2, h, v1Next, v2Next, v1Next - vn1, v2Next - vn2, C1 - oldC1, C2 - oldC2)

        return xn, vn1, vn2, h
    elif abs(S) < eps / 2 ** (p + 1):
        C1 += 1

        saveCurrentValuesSystem(S1, S2, xn, vn1, vn2, h, v1Next, v2Next, v1Next - vn1, v2Next - vn2, C1 - oldC1, C2 - oldC2)
        return xn, vn1, vn2, h * 2
    else:
        while abs(S) > eps:
            C2 += 1
            h /= 2
            xn, vn1, vn2 = stepForSystem(x, v1, v2, h, f1, f2, True)
            xHalf, v1Half, v2Half = stepForSystem(x, v1, v2, h / 2, f1, f2, True)
            xNext, v1Next, v2Next = stepForSystem(x, v1Half, v2Half, h / 2, f1, f2, True)
            S1 = (v1Next - vn1) / (2 ** p - 1)
            S2 = (v2Next - vn2) / (2 ** p - 1)
            S = max(abs(S1), abs(S2))

        saveCurrentValuesSystem(S1, S2, xn, vn1, vn2, h, v1Next, v2Next, v1Next - vn1, v2Next - vn2, C1 - oldC1, C2 - oldC2)
        return xn, vn1, vn2, h


def RK4Sys(x, v1, v2, h, Nmax, b, e, f1, f2):
    saveCurrentValuesSystem(0, 0, x, v1, v2, h, v1, v2, 0, 0, 0, 0)

    xArr = []
    v1Arr = []
    v2Arr = []

    xArr.append(x)
    v1Arr.append(v1)
    v2Arr.append(v2)

    for i in range(1, Nmax + 1):
        x, v1, v2 = stepForSystem(x, v1, v2, h, f1, f2)
        xHalf, v1Half, v2Half = stepForSystem(x, v1, v2, h / 2, f1, f2, True)
        xNext, v1Next, v2Next = stepForSystem(x, v1Half, v2Half, h / 2, f1, f2, True)

        S1 = (v1Next - v1) / (2 ** p - 1)
        S2 = (v2Next - v2) / (2 ** p - 1)
        S = max(abs(S1), abs(S2))
        
        
        if f1(x, v1, v2) > maximum_derivative or f2(x, v1, v2) > maximum_derivative:
            xArr.append(x)
            v1Arr.append(v1)
            v2Arr.append(v2)
            return xArr, v1Arr, v2Arr
        if x >= b - e and x <= b:
            xArr.append(x)
            v1Arr.append(v1)
            v2Arr.append(v2)
            return xArr, v1Arr, v2Arr
        if x > b:
            x, v1, v2 = stepForSystem(xArr[i - 1], v1Arr[i - 1], v2Arr[i - 1], b - xArr[i - 1], f1, f2)
            xArr.append(x)
            v1Arr.append(v1)
            v2Arr.append(v2)
            eraseEndValues()
            xi[len(xi) - 1] = x
            u1[len(u1) - 1] = v1
            u2[len(u2) - 1] = v2
            hi[len(hi) - 1] = b - xArr[i - 1]
            over_bound = True
            

            return xArr, v1Arr, v2Arr
        xArr.append(x)
        v1Arr.append(v1)
        v2Arr.append(v2)
    return xArr, v1Arr, v2Arr


def RK4WCSys(x, v1, v2, h, Nmax, b, e, f1, f2, eps):
    saveCurrentValuesSystem(0, 0, x, v1, v2, h, v1, v2, 0, 0, 0, 0)

    xArr = []
    v1Arr = []
    v2Arr = []

    xArr.append(x)
    v1Arr.append(v1)
    v2Arr.append(v2)

    for i in range(1, Nmax + 1):
        x, v1, v2, h = stepForSystemWithControl(x, v1, v2, h, f1, f2, eps)
        if f1(x, v1, v2) > maximum_derivative or f2(x, v1, v2) > maximum_derivative:
            xArr.append(x)
            v1Arr.append(v1)
            v2Arr.append(v2)
            return xArr, v1Arr, v2Arr
        if h < hmin:
            xArr.append(x)
            v1Arr.append(v1)
            v2Arr.append(v2)
            return xArr, v1Arr, v2Arr
        if x >= b - e and x <= b:
            xArr.append(x)
            v1Arr.append(v1)
            v2Arr.append(v2)
            return xArr, v1Arr, v2Arr
        if x > b:
            x, v1, v2 = stepForSystem(xArr[i - 1], v1Arr[i - 1], v2Arr[i - 1], b - xArr[i - 1], f1, f2, True)
            xArr.append(x)
            v1Arr.append(v1)
            v2Arr.append(v2)
            eraseEndValues()
            xi[len(xi) - 1] = x
            u1[len(u1) - 1] = v1
            u2[len(u2) - 1] = v2
            hi[len(hi) - 1] = b - xArr[i - 1]
            over_bound = True

            return xArr, v1Arr, v2Arr
        xArr.append(x)
        v1Arr.append(v1)
        v2Arr.append(v2)
    return xArr, v1Arr, v2Arr


catchParamsFromCmd()

startCalculation()

saveToDatabase()
