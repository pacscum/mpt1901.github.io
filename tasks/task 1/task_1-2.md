*[На главную](https://mpt1901.github.io/)*

# Упражнение 1.2

Данные по композиционному анализу имеются в очень урезанном виде, отсутствует плотность остатка, имеется только плотность и состав разгазированной нефти:

Состав разгазированной нефти:

|     Компонент      | Мольная концентрация, % |Плотность при стандартных условиях, кг/куб.м|Массовая доля, г/моль|
| :----------------: | :----------------------: |---|---|
|       $N_2$        |          0.001           |
|       $CO_2$       |          0.058           |
|       $C_1$        |          0.348           |
|       $C_2$        |          0.378           |
|       $C_3$        |          0.983           |
|       $iC_4$       |          0.417           |
|       $nC_4$       |          1.472           |
|       $iC_5$       |          1.203           |
|       $nC_5$       |          2.077           |
|       $C_6$        |          4.866           |
|       $C_7$        |          10.416          |
|       $C_8$        |          12.013          |
|       $C_9$        |          7.745           |
|     $C_{10}+$      |          58.023          |

 **Молярная масса смеси = 187.01 г/моль**

**Плотность разгазированной нефти $\rho_{st}=0.836 \ г/см^3$**

&nbsp;

**Необходимо определить плотность остатка $C_{10}+$**

## Алгоритм расчета
1. Определить плотность каждого компонента при стандартных условиях.

> Источники информации:
> 
> * [Definition and molecular weight (molar mass) of some common substances
> ](https://www.engineeringtoolbox.com/molecular-weight-gas-vapor-d_1156.html)
> * [Molweight, melting and boiling point, density, flash point and autoignition temperature, as well as number of carbon and hydrogen
> atoms in each molecule are given for 200 different hydrocarbons
> ](https://www.engineeringtoolbox.com/hydrocarbon-boiling-melting-flash-autoignition-point-density-gravity-molweight-d_1966.html)
> * [Gas Density, Molecular Weight and Density](http://www.teknopoli.com/PDF/Gas_Density_Table.pdf)

3. Определение массовой доли $\omega_{i}$ для каждого i-го компонента:
    $$
    \omega_{i} = \dfrac{a_{i}\times M_{i}}{M_c}
    \tag{1}
    $$
   > где $a_i$ - мольная концентрация.
4. Определение плотности остатка $\rho_{C_{10}+}$ выполняется аналогично пункту 2 упражнения № 1.1 по формуле:
$$
  \rho_c=\sum_{i=1}^{N}{\omega_i\times \rho_i}
    \tag{2}
$$
