import math
import pprint


R = 0.0083675
temperature = 293

#          N2   CO2 H2S     CH4     C2H6    C3H8    nC4H10  nC5H12  nC6H14  nC7H16  nC8H18  nC9H20  nC10H22 
a_list = [[0,   0,  0.13,   0.025,  0.01,   0.09,   0.095,  0.1,    0.11,   0.115,  0.12,   0.12,   0.125], # 0
          [0,   0,  0.15,   0.105,  0.13,   0.125,  0.115,  0.115,  0.115,  0.115,  0.115,  0.115,  0.115], # 1
          [0,   0,  0,      0.07,   0.085,  0.08,   0.075,  0.07,   0.07,   0.06,   0.06,   0.06,   0.055], # 2
          [0,   0,  0,      0,      0.005,  0.01,   0.025,  0.03,   0.03,   0.035,  0.04,   0.04,   0.045], # 3
          [0,   0,  0,      0,      0,      0.005,  0.01,   0.01,   0.02,   0.02,   0.02,   0.02,   0.02],  # 4
          [0,   0,  0,      0,      0,      0,      0,      0.02,   0.005,  0.005,  0.005,  0.005,  0.005], # 5
          [0,   0,  0,      0,      0,      0,      0,      0.005,  0.005,  0.005,  0.005,  0.005,  0.005], # 6
          [0,   0,  0,      0,      0,      0,      0,      0,      0,      0,      0,      0,      0,],    # 7
          [0,   0,  0,      0,      0,      0,      0,      0,      0,      0,      0,      0,      0,],    # 8
          [0,   0,  0,      0,      0,      0,      0,      0,      0,      0,      0,      0,      0,],    # 9
          [0,   0,  0,      0,      0,      0,      0,      0,      0,      0,      0,      0,      0,],    # 10
          [0,   0,  0,      0,      0,      0,      0,      0,      0,      0,      0,      0,      0,],    # 11
          [0,   0,  0,      0,      0,      0,      0,      0,      0,      0,      0,      0,      0,],    # 12
          ]

omega_list = [0.04, 0.231, 0.013, 0.108, 0.182, 0.201, 0.192, 0.252, 0.208]

mole_fractions = [
    0,      #N2
    0,      #CO2
    0,      #H2S
    0.9,      #CH4
    0.1,      #C2H6
    0,      #C3H8
    0,      #nC4H10
    0,      #nC5H12
    0,      #nC6H14
    0,      #nC7H16
    0,      #nC8H18
    0,      #nC9H20
    0,      #nC10H22
]

temp_critical = [126.3, 304.2, 190.55, 305.43, 369.82, 425.2, 407.2, 470.4, 461]
pressure_critical = [3.4, 7.381, 4.7, 4.9, 4.3, 3.8, 3.7, 3.4, 3.3]

a_crit = []
def set_a_crit():
    for index in range(len(mole_fractions)):
        if mole_fractions[index] == 0:
            a_crit.append(0)
        else:
            current_element = 0.457235*(R**2)*(temp_critical[index]**2)/pressure_critical[index]
            a_crit.append(current_element)
    return a_crit

b = []
def set_b():
    for index in range(len(mole_fractions)):
        if mole_fractions[index] == 0:
            b.append(0)
        else:
            current_element = 0.077796*R*temp_critical[index]/pressure_critical[index]
            b.append(current_element)
    return b


m = []
def set_m():
    for index in range(len(mole_fractions)):
        if mole_fractions[index] == 0:
            m.append(0)
        else:
            current_element = 0.37464+1.54226*omega_list[index]-0.26992*(omega_list[index]**2)
            m.append(current_element)
    return m

alpha_t = []
def set_alpha_t():
    for index in range(len(mole_fractions)):
        if mole_fractions[index] == 0:
            alpha_t.append(0)
        else:
            current_element = (1+m[index]*(1-math.sqrt(temperature/temp_critical[index])))**2
            alpha_t.append(current_element)
    return alpha_t

a = []
def set_a():

    for index in range(len(mole_fractions)):
        if mole_fractions[index] == 0:
            a.append(0)
        else:
            current_element = a_crit[index]*alpha_t[index]
            a.append(current_element)
    return a


def set_a_mix():
    a_mix = 0.0
    for i in range(len(mole_fractions)):
        for j in range(len(mole_fractions)):
            a_mix += mole_fractions[i]*mole_fractions[j]*(1-a_list[i][j])*math.sqrt(a[i]*a[j])
    return a_mix


def calc():
    b = set_b()
    a_crit = set_a_crit()
    m = set_m()
    alpha_t = set_alpha_t()
    a = set_a()
    a_mix = set_a_mix()

    print(a_mix)
    print(a_list[2][12])

calc()
