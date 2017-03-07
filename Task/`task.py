import random
import math

PI = math.acos(-1)
cnt = 0
p = (0.12, 1.18, 0.17)
f = 0

def write(x, y, z, s):
	global f
	global cnt
	f.write('object sphere\n')
	f.write('name sphere(%d)\n' % cnt)
	f.write('position %f %f %f\n' % (x + p[0], y + p[1], z + p[2]))
	f.write('scale %f %f %f\n' % (s, s, s))
	f.write('end\n\n')
	cnt += 1

def cubic():
	f = open('cubic5x5.conf', 'w')
	for x in range(-3, 6, 2):
		for y in range(-1, 8, 2):
			for z in range(4, 13, 2):
				write(x, y, z, 0.3)
	f.close()
def near():
	f = open('near.conf', 'w')
	for i in range(5):
		x = random.random() * 2 - 1
		y = random.random() * 2
		z = random.random() * 1 + 0.3
		write(x, y, z, 0.2)
	f.close()
def radiate():
	f = open('radiate.conf', 'w')
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
	f.close()
def parade():
	f = open('parade.conf', 'w')
	for z in range(1, 11, 1):
		write(-1, 1.2, z * 1.5, 0.2)
		write( 1, 1.2, z * 1.5, 0.2)
	f.close()

def density():
	global f
	rs = [0.0, 0.5, 1.0, 1.5, 2.0, 4.0]
	for r in rs:
		dis = 0.5 * (1 + r)
		f = open('density-' + str(r) + '.conf', 'w')
		for i in range(-4, 5, 1):
			for j in range(-4, 5, 1):
				x = p[0] + i * dis
				y = p[1] + j * dis
				write(x, y, 15, 0.5)
		f.close()
def depth():
	global f
	ds = [(2.5, 5), (2.5, 10), (2.5, 50), (5, 10), (5, 50), (10, 50)]
	for d in ds:
		f = open('depth-(' + str(d[0]) + ',' + str(d[1]) + ').conf', 'w')
		for i in range(0, 12):
			z = d[i % 2]
			x = z / 2 * math.cos(i / 6 * PI)
			y = z / 2 * math.sin(i / 6 * PI)
			write(x, y, z, 0.5)				
		f.close()
def occlusion():
	global f
	ts = [(2,2.5), (3,2.5), (2,3.5), (3,3.5)]
	for t in ts:
		level = t[0]
		an = t[1]
		f = open('occlusion-(' + str(t[0]) + ',' + str(t[1]) + ').conf', 'w')
		z0 = 5
		s = [0.75, 0.5, 0.25]
		disq = {}
		disq[2.5] = [0.0, 1.0, 1.75]
		disq[3.5] = [0.0, 1.0, 1.5]
		zq = [0.0, 1.0, 2.0]
		write(0, 0, z0, s[0])
		for l in range(1, level):
			z = z0 + zq[l]
			dis = z * math.sin(an / 180.0 * PI) * disq[an][l]
			for dire in range(0, 4, 1):
				x = dis * math.cos(dire * PI / 2)
				y = dis * math.sin(dire * PI / 2)
				write(x, y, z, s[l])
				# write(x, y, z, 0.5)
		f.close()


occlusion()