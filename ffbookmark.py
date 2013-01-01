#!/usr/bin/python

import sys
import re
from bs4 import BeautifulSoup

def main(filepath):
	regex_fixdt = r'</A>'
	regex_fixdd = r'(?<!\>)\n<DT>'
	print 'Parsing file',filepath
	
	# Try to open the file passed as an argument
	# If it does not exists, or cannot be open, an IOError is thrown
	try:
		# We need the file contents before BeautifulSoup'ing it, because it chokes on this malformed HTML (Too much recursion)
		with open(filepath) as file: 
			filecontent = file.read()

			# Fixing HTML Time !
			# <DT> and <DD> tags are never closed
			filecontent = re.sub(regex_fixdt, fixdt, filecontent)
			filecontent = re.sub(regex_fixdd, fixdd, filecontent)

			soup = BeautifulSoup(filecontent)
			print soup.prettify()
	except IOError as e:
		print "[Error]",e.strerror
	
def fixdt(matchobj):
	if (matchobj.group(0)) == '</A>': return '</a></dt>'
	else: return ''

def fixdd(matchobj):
	if (matchobj.group(0)): return '</dd>\n<dt>'
	else: return ''

if __name__ == "__main__":
	if len(sys.argv) > 1:
		main(str(sys.argv[1]))
	else:
		print 'No file to parse'
