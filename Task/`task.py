import random
import math

cnt = 0
p = (0.12, 1.18, 0.17)

def write(x, y, z, s):
	global f
	global cnt
	f.write('object sphere\n')
	f.write('name sphere(%d)\n' % cnt)
	f.write('position %f %f %f\n' % (x, y, z))
	f.write('scale %f %f %f\n' % (s, s, s))
	f.write('end\n\n')
	cnt += 1

'''f = open('cubic5x5.conf', 'w')
f.write('trials 50\n\n')
for x in range(-3, 6, 2):
	for y in range(-1, 8, 2):
		for z in range(4, 13, 2):
			write(x, y, z, 0.3)
f.close()'''

'''f = open('near.conf', 'w')
f.write('trials 10\n\n')
for i in range(5):
	x = random.random() * 2 - 1
	y = random.random() * 2
	z = random.random() * 1 + 0.3
	write(x, y, z, 0.2)
f.close()'''

'''f = open('radiate.conf', 'w')
f.write('trials 50\n\n');
PI = math.acos(-1)
for depth in range(5, 10, 1):
	directA = -PI/2 + 0.9 - 0.1 * depth
	directB =  PI/2 - 0.9 + 0.1 * depth
	ii = 2 + depth
	for i in range(0, ii + 1):
		direct = directA + (directB - directA) / ii * i
		riseA = 0.4 - depth * 0.1
		riseB = PI/3 + 0.5 - depth * 0.1
		print(riseA * 180 / PI, riseB * 180 / PI)
		jj = 3
		for j in range(0, jj + 1):
			rise = riseA + (riseB - riseA) / jj * j
			x = depth * math.cos(rise) * math.sin(direct)
			y = depth * math.sin(rise)
			z = depth * math.cos(rise) * math.cos(direct)
			write(x, y, z + 3, 0.3)
f.close()'''

'''f = open('plane9x9.conf', 'w')
f.write('trials 50\n\n')
for x in range(-10, 11, 2):
	for y in range(-9, 12, 2):
		write(x, y, 20, 0.75)
f.close()'''

f = open('test.conf', 'w')
f.write('trials 10\n\n')
for z in range(1, 11, 1):
	write(-1, 1.2, z * 1.5, 0.2)
	write( 1, 1.2, z * 1.5, 0.2)
f.close()