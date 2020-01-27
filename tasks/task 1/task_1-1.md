# Упражнение № 1.1
Имеются данные по композиционному составу в урезанном виде:

Состав пластовой нефти:

|Компонент|Молярная концентрация, %|
|:-------:|:----------------------:|
|$N_2$|0.545|
|$CO_2$|2.821|
|$C_1$|55.465|
|$C_2$|8.58|
|$C_3$|5.736|
|$iC_4$|1.008|
|$nC_4$|2.433|
|$iC_5$|0.896|
|$nC_5$|1.242|
|$C_6$|1.587|
|$C_7$|2.566|
|$C_8$|2.764|
|$C_9$|1.71|
|$C_{10}+$|12.647|
|**Молярная масса**|**59.7**|

&nbsp;

---

Необходимо рассчитать:

1. молярную массу остатка $C_{10}+$
2. массовые доли компонентов
> Использовать таблицы Katz & Firoozabadi

> Плотность дегазированной нефти $\rho_{st} = 0.836 \ г/см^3$

Источники информации:

* [Definition and molecular weight (molar mass) of some common substances ](https://www.engineeringtoolbox.com/molecular-weight-gas-vapor-d_1156.html)
* [Molweight, melting and boiling point, density, flash point and autoignition temperature, as well as number of carbon and hydrogen atoms in each molecule are given for 200 different hydrocarbons ](https://www.engineeringtoolbox.com/hydrocarbon-boiling-melting-flash-autoignition-point-density-gravity-molweight-d_1966.html)
* [Gas Density, Molecular Weight and Density](http://www.teknopoli.com/PDF/Gas_Density_Table.pdf)


---

## Алгоритм расчета

1. Определение молярной массы $M_i$ каждого компонента.
2. Определение молярной массы $C_{10}+$ компонента:
   
    Молярная масса смеси определяется как сумма произведений мольной концентрации каждого компонента на его молярную массу:
    $$
    M_c=\sum_{i=1}^{N}{a_i\times M_i}
    \tag{1}
    $$

    где N - количество компонентов;  
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp; $a_i$ - мольная концентрация.


    Для нахождения молярной массы компонента $C_{10}+$ уравнение (1) можно переписать в виде:
    $$
    M_c=\sum^{N-1}_{i=1}{a_i\times M_i} \ + \ a_{C_{10}+}\times M_{C_{10}+}
    \tag{2}
    $$
    Отсюда, можно найти молярную массу $C_{10}+$:
    $$
    M_{c(10+)}=\dfrac{M_c-\sum^{N-1}_{i=1}{a_i\times M_i}}{a_{C_{10}+}}
    \tag{3}
    $$
3. Определение массовой доли $\omega_{C_{10}+}$:
    $$
    \omega_{C_{10}+} = \dfrac{z_{C_{10}+}\times M_{C_{10}+}}{M_c}
    \tag{4}
    $$