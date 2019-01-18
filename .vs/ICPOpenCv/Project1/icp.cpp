#include <opencv2\core.hpp>
#include <opencv2\highgui.hpp>
//#include <opencv2\opencv.hpp>
#include <iostream>
#include <stdio.h>
#include <stdarg.h>
#include <stdlib.h>
#include <string.h>
#include <malloc.h>
#include <math.h>
#include <fstream>

using namespace std;
using namespace cv;

extern "C" void __declspec(dllexport) __stdcall icpGetCurrentICPModel(double ModelPointsArr[3][3], double DataPointsArr[3][3], double (&transformedPointsArr)[3][3])
{
	Mat ModelPoints = Mat::zeros(3, 3, CV_64FC1);
	Mat DataPoints = Mat::zeros(3, 3, CV_64FC1);

	for (int i = 0; i < 3; i++) {
		ModelPoints.at<double>(i, 0) = ModelPointsArr[i][0];
		ModelPoints.at<double>(i, 1) = ModelPointsArr[i][1];
		ModelPoints.at<double>(i, 2) = ModelPointsArr[i][2];

		DataPoints.at<double>(i, 0) = DataPointsArr[i][0];
		DataPoints.at<double>(i, 1) = DataPointsArr[i][1];
		DataPoints.at<double>(i, 2) = DataPointsArr[i][2];
	}

	//cout << "ModelPoints=\n" << ModelPoints << endl;
	//cout << "DataPoints=\n" << DataPoints << endl;


	Mat R = Mat::eye(3, 3, CV_64FC1);
	Mat T = Mat::zeros(3, 1, CV_64FC1);
	Mat mem_vec = Mat::zeros(3, 1, CV_64FC1);
	Mat med_vec = Mat::zeros(3, 1, CV_64FC1);

	int md = 3;

	//###############################################
	//mem=mean(model,2);
	//###############################################
	for (int i = 0; i < 3;i++)
	{
		mem_vec.at<double>(0, 0) += ModelPoints.at<double>(0, i);
		mem_vec.at<double>(1, 0) += ModelPoints.at<double>(1, i);
		mem_vec.at<double>(2, 0) += ModelPoints.at<double>(2, i);
	}
	mem_vec.at<double>(0, 0) = mem_vec.at<double>(0, 0) / 3;
	mem_vec.at<double>(1, 0) = mem_vec.at<double>(1, 0) / 3;
	mem_vec.at<double>(2, 0) = mem_vec.at<double>(2, 0) / 3;

	for (int i = 0; i < 3;i++)
	{
		med_vec.at<double>(0, 0) += DataPoints.at<double>(0, i);
		med_vec.at<double>(1, 0) += DataPoints.at<double>(1, i);
		med_vec.at<double>(2, 0) += DataPoints.at<double>(2, i);
	}
	med_vec.at<double>(0, 0) = med_vec.at<double>(0, 0) / 3;
	med_vec.at<double>(1, 0) = med_vec.at<double>(1, 0) / 3;
	med_vec.at<double>(2, 0) = med_vec.at<double>(2, 0) / 3;

	//cout << "mem_vec=\n" << mem_vec << endl;
	//cout << "med_vec=\n" << med_vec << endl;

   //A=transformed_data-med*ones(1,md);
   //B=model(:,iclosest)-mem*ones(1,md);
   //###########################################
	Mat A = Mat(3, md, CV_64FC1);
	Mat B = Mat(3, md, CV_64FC1);
	
	for (int i = 0; i < md; i++) {
		A.at<double>(0, i) = DataPoints.at<double>(0, i) - med_vec.at<double>(0, 0);
		A.at<double>(1, i) = DataPoints.at<double>(1, i) - med_vec.at<double>(1, 0);
		A.at<double>(2, i) = DataPoints.at<double>(2, i) - med_vec.at<double>(2, 0);

		B.at<double>(0, i) = ModelPoints.at<double>(0, i) - mem_vec.at<double>(0, 0);
		B.at<double>(1, i) = ModelPoints.at<double>(1, i) - mem_vec.at<double>(1, 0);
		B.at<double>(2, i) = ModelPoints.at<double>(2, i) - mem_vec.at<double>(2, 0);

	}
		
	//###################################
	//compute the rotation update
	//[U,S,V] = svd(B*A');
	//U(:,end)=U(:,end)*det(U*V');
	//dR=U*V';
	//####################################
	Mat U = Mat(3, 3, CV_64FC1);
	Mat W = Mat(3, 3, CV_64FC1);
	Mat Vt = Mat(3, 3, CV_64FC1);
	Mat BAt = Mat (3, 3, CV_64FC1);	
	BAt = B * A.t();

	SVD::compute(BAt, W, U, Vt);

	Mat UVt = Mat(3, 3, CV_64FC1);
	UVt = U * Vt;

	double detUVT = determinant(UVt);

	U.at<double>(0, 2) = U.at<double>(0, 2)*detUVT;
	U.at<double>(1, 2) = U.at<double>(1, 2)*detUVT;
	U.at<double>(2, 2) = U.at<double>(2, 2)*detUVT;


	//d=U*V';
	R = U * Vt;

	// Compute the translation update
	//dT=(mem-dR*med);
	T = mem_vec - R * med_vec;

	cout << "R=\n" << R << endl;
	cout << "T=\n" << T << endl;

	Mat transformedToJoint = Mat::zeros(3, 1, CV_64FC1);
	Mat pivotPoint = Mat::zeros(3, 1, CV_64FC1);

	// TransformedPoint = R * AnchorPoint + T
	for (int i = 0; i < 3; i++) {
		pivotPoint.at<double>(0, 0) = DataPointsArr[i][0];
		pivotPoint.at<double>(1, 0) = DataPointsArr[i][1];
		pivotPoint.at<double>(2, 0) = DataPointsArr[i][2];
		cout << "P=\n" << pivotPoint << endl;

		transformedToJoint = R * pivotPoint + T;

		cout << "t:\n" << transformedToJoint << endl;
		transformedPointsArr[i][0] = transformedToJoint.at<double>(0, 0);
		transformedPointsArr[i][1] = transformedToJoint.at<double>(1, 0);
		transformedPointsArr[i][2] = transformedToJoint.at<double>(2, 0);
	}
}

//


int main()
{
	double ModelPointsArr[3][3] = { {0.05856052, - 0.2821604, 1.236075}, { 0.1929782, - 0.279166, 1.230189} , {0.06356425, - 0.5588056, 1.162657} };
	double DataPointsArr[3][3] = { {-0.94875470, 3.581969, 744.605} , {-0.94875470, 3.858615, 744.605} , {-1.015964, 3.858615, 744.605} };
	double transformedPointsArr[3][3];

	icpGetCurrentICPModel(ModelPointsArr, DataPointsArr, transformedPointsArr);

	getchar();
	return 0;
}

