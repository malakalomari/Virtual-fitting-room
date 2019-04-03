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
		ModelPoints.at<double>(0,i) = ModelPointsArr[0][i];
		ModelPoints.at<double>(1,i) = ModelPointsArr[1][i];
		ModelPoints.at<double>(2,i) = ModelPointsArr[2][i];

		DataPoints.at<double>(0,i) = DataPointsArr[0][i];
		DataPoints.at<double>(1,i) = DataPointsArr[1][i];
		DataPoints.at<double>(2,i) = DataPointsArr[2][i];
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

	cout << "det R=\n" << determinant(R) << endl;

	Mat transformedToJoint = Mat::zeros(3, 1, CV_64FC1);
	Mat pivotPoint = Mat::zeros(3, 1, CV_64FC1);
	Mat modelPoint = Mat::zeros(3, 1, CV_64FC1);

	// TransformedPoint = R * AnchorPoint + T
	for (int i = 0; i < 3; i++) {
		pivotPoint.at<double>(0, 0) = DataPointsArr[0][i];
		pivotPoint.at<double>(1, 0) = DataPointsArr[1][i];
		pivotPoint.at<double>(2, 0) = DataPointsArr[2][i];

		modelPoint.at<double>(0, 0) = ModelPointsArr[0][i];
		modelPoint.at<double>(1, 0) = ModelPointsArr[1][i];
		modelPoint.at<double>(2, 0) = ModelPointsArr[2][i];

		cout << "Pivot "<<i <<"=\n" << pivotPoint << endl;

		transformedToJoint = R * pivotPoint + T;

		cout << "transformed pivot " << i << ":\n" << transformedToJoint << endl;

		cout << "model " << i << "=\n" << modelPoint << endl;

		cout << "Error " << i << "\n" << transformedToJoint - modelPoint << endl;
	
		transformedPointsArr[0][i] = transformedToJoint.at<double>(0, 0);
		transformedPointsArr[1][i] = transformedToJoint.at<double>(1, 0);
		transformedPointsArr[2][i] = transformedToJoint.at<double>(2, 0);
	}
}

int main()
{
	double ModelPointsArr[3][3] = {
	{0.004785966 , 0.1325456,  0.2622803 },
	{-0.3357168 , -0.3308465 , -0.4949186} ,
	{1.650084   ,  1.650084 ,  1.650084} };


	double DataPointsArr[3][3] = {
	{1.70069651305676    , 1.28069667518139  , 1.70069722831249} ,
	{-12.3507070094347   , -12.3507070094347, -6.26582713425159} ,
	{700                 , 700              ,  700             } 
	};

	double transformedPointsArr[3][3];

	icpGetCurrentICPModel(ModelPointsArr, DataPointsArr, transformedPointsArr);

	getchar();
	return 0;
}

